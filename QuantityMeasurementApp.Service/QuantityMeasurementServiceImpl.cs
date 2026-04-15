using System;
using QuantityMeasurementApp.Models;
using QuantityMeasurementApp.Repository;
using QuantityMeasurementApp.Exceptions;
using QuantityMeasurementApp.Utilities;

namespace QuantityMeasurementApp.Service
{
    public class QuantityMeasurementServiceImpl : IQuantityMeasurementService
    {
        private readonly IQuantityMeasurementRepository repository;
        private readonly RedisCacheService _cache;

        public QuantityMeasurementServiceImpl(IQuantityMeasurementRepository repo, RedisCacheService cache)
        {
            repository = repo;
            _cache = cache;
        }

        public QuantityDTO Convert(QuantityDTO source, string targetUnit)
        {
            var entity = CreateEntity(source, null, "CONVERT");
            string cacheKey = $"convert_{source.Value}_{source.Unit}_{targetUnit}";

            try
            {
                var cachedResult = _cache.GetAsync<QuantityDTO>(cacheKey).Result;
                if (cachedResult != null) return cachedResult;

                QuantityDTO result = ExecuteSingleOperandOperation(source, (q) =>
                {
                    if (TryParseUnit(source.Unit, out Enum? sourceEnumUnit, out Type? enumType)
                        && sourceEnumUnit != null && enumType != null)
                    {
                        try
                        {
                            var targetEnum = (Enum)Enum.Parse(enumType, targetUnit, true);

                            var quantityType = typeof(Quantity<>).MakeGenericType(enumType);
                            var quantityInstance = Activator.CreateInstance(quantityType, source.Value, sourceEnumUnit);

                            var convertMethod = quantityType.GetMethod("ConvertTo");
                            var resultQuantity = convertMethod!.Invoke(quantityInstance, new object[] { targetEnum });

                            double val = (double)resultQuantity!.GetType().GetProperty("Value")!.GetValue(resultQuantity)!;
                            return new QuantityDTO(val, targetUnit);
                        }
                        catch (ArgumentException) { throw new QuantityMeasurementException($"Target unit '{targetUnit}' is not valid."); }
                    }
                    throw new QuantityMeasurementException($"Source unit '{source.Unit}' is not recognized.");
                });

                _cache.SetAsync(cacheKey, result).Wait();
                entity.Result = result;
                repository.Save(entity);
                return result;
            }
            catch (Exception ex)
            {
                entity.ErrorMessage = ex.Message;
                repository.Save(entity);
                if (ex is QuantityMeasurementException) throw;
                throw new QuantityMeasurementException($"Conversion failed: {ex.Message}");
            }
        }

        public bool Compare(QuantityDTO q1, QuantityDTO q2)
        {
            return ExecuteDoubleOperandOperation(q1, q2, (inst1, inst2) =>
            {
                var equalsMethod = inst1.GetType().GetMethod("Equals");
                return (bool)equalsMethod!.Invoke(inst1, new[] { inst2 })!;
            });
        }

        public QuantityDTO Add(QuantityDTO q1, QuantityDTO q2, string? targetUnit = null)
        {
            return ExecuteDoubleOperandOperation(q1, q2, (inst1, inst2) =>
            {
                object? resultQuantity;
                if (string.IsNullOrEmpty(targetUnit))
                {
                    var addMethod = inst1.GetType().GetMethod("Add", new[] { inst1.GetType() });
                    resultQuantity = addMethod!.Invoke(inst1, new[] { inst2 });
                }
                else
                {
                    TryParseUnit(targetUnit, out Enum? targetEnum, out Type? targetType);
                    var addMethod = inst1.GetType().GetMethod("Add", new[] { inst1.GetType(), targetEnum!.GetType() });
                    resultQuantity = addMethod!.Invoke(inst1, new[] { inst2, targetEnum });
                }

                double val = (double)resultQuantity!.GetType().GetProperty("Value")!.GetValue(resultQuantity)!;
                string unit = resultQuantity.GetType().GetProperty("Unit")!.GetValue(resultQuantity)!.ToString()!;
                return new QuantityDTO(val, unit);
            });
        }

        public QuantityDTO Subtract(QuantityDTO q1, QuantityDTO q2, string? targetUnit = null)
        {
            return ExecuteDoubleOperandOperation(q1, q2, (inst1, inst2) =>
            {
                object? resultQuantity;
                if (string.IsNullOrEmpty(targetUnit))
                {
                    var subMethod = inst1.GetType().GetMethod("Subtract", new[] { inst1.GetType() });
                    resultQuantity = subMethod!.Invoke(inst1, new[] { inst2 });
                }
                else
                {
                    TryParseUnit(targetUnit, out Enum? targetEnum, out Type? targetType);
                    var subMethod = inst1.GetType().GetMethod("Subtract", new[] { inst1.GetType(), targetEnum!.GetType() });
                    resultQuantity = subMethod!.Invoke(inst1, new[] { inst2, targetEnum });
                }

                double val = (double)resultQuantity!.GetType().GetProperty("Value")!.GetValue(resultQuantity)!;
                string unit = resultQuantity.GetType().GetProperty("Unit")!.GetValue(resultQuantity)!.ToString()!;
                return new QuantityDTO(val, unit);
            });
        }

        public double Divide(QuantityDTO q1, QuantityDTO q2)
        {
            return ExecuteDoubleOperandOperation(q1, q2, (inst1, inst2) =>
            {
                var divMethod = inst1.GetType().GetMethod("Divide", new[] { inst1.GetType() });
                return (double)divMethod!.Invoke(inst1, new[] { inst2 })!;
            });
        }

        public double Multiply(QuantityDTO q1, QuantityDTO q2)
        {
            // Scalar cross product of base values
            return ExecuteDoubleOperandOperation(q1, q2, (inst1, inst2) =>
            {
                var method = inst1.GetType().GetMethod("ConvertToBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                double b1 = (double)method!.Invoke(inst1, null)!;
                double b2 = (double)method!.Invoke(inst2, null)!;
                return b1 * b2;
            });
        }

        private T ExecuteDoubleOperandOperation<T>(QuantityDTO q1, QuantityDTO q2, Func<object, object, T> operation)
        {
            if (TryParseUnit(q1.Unit, out Enum? unit1, out Type? type1) &&
                TryParseUnit(q2.Unit, out Enum? unit2, out Type? type2))
            {
                if (type1 != type2)
                    throw new QuantityMeasurementException($"Cannot operate on {q1.Unit} and {q2.Unit} as they are different measurement types.");

                var quantityType = typeof(Quantity<>).MakeGenericType(type1!);
                var instance1 = Activator.CreateInstance(quantityType, q1.Value, unit1);
                var instance2 = Activator.CreateInstance(quantityType, q2.Value, unit2);

                return operation(instance1!, instance2!);
            }
            throw new QuantityMeasurementException($"Invalid units: {q1.Unit} or {q2.Unit}");
        }

        private QuantityMeasurementEntity CreateEntity(QuantityDTO q1, QuantityDTO? q2, string op)
        {
            return new QuantityMeasurementEntity(q1, op) { Operand2 = q2 };
        }

        private T ExecuteSingleOperandOperation<T>(QuantityDTO q, Func<QuantityDTO, T> operation)
        {
            return operation(q);
        }

        private bool TryParseUnit(string unitStr, out Enum? unit, out Type? enumType)
        {
            if (Enum.TryParse<LengthUnit>(unitStr, true, out var l)) { unit = l; enumType = typeof(LengthUnit); return true; }
            if (Enum.TryParse<WeightUnit>(unitStr, true, out var w)) { unit = w; enumType = typeof(WeightUnit); return true; }
            if (Enum.TryParse<VolumeUnit>(unitStr, true, out var v)) { unit = v; enumType = typeof(VolumeUnit); return true; }
            if (Enum.TryParse<TemperatureUnit>(unitStr, true, out var t)) { unit = t; enumType = typeof(TemperatureUnit); return true; }

            unit = null; enumType = null; return false;
        }
    }
}
