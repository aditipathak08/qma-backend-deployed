using System;

namespace QuantityMeasurementApp.Models
{
    /// <summary>
    /// Represents a generic physical quantity with a numeric value and a specific unit.
    /// Supports equality comparison, unit conversion, and basic arithmetic operations
    /// across different units within the same measurement category (e.g., Length, Weight).
    /// Why: To provide a single, reusable class for all measurement types while ensuring type safety through generics.
    /// How: Uses generics (U) constrained to Enums and employs reflection to dynamically access unit-specific conversion logic.
    /// </summary>
    /// <typeparam name="U">The type of unit enum (e.g., LengthUnit, WeightUnit).</typeparam>
    public class Quantity<U> where U : Enum
    {
        private readonly double value;
        private readonly U unit;

        // Tolerance for double comparison to handle precision issues (Floating point errors).
        private const double EPSILON = 1e-6;

        /// <summary>
        /// Initializes a new instance of the <see cref="Quantity{U}"/> class.
        /// Why: Ensures that all quantities are created with valid values and units from the start.
        /// </summary>
        /// <param name="value">The numeric value of the quantity.</param>
        /// <param name="unit">The unit of measurement.</param>
        /// <exception cref="ArgumentException">Thrown when value is NaN or Infinity, or unit is null.</exception>
        public Quantity(double value, U unit)
        {
            if (unit == null)
                throw new ArgumentException("Unit cannot be null");

            // Prevent invalid arithmetic results by restricting to finite numbers.
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Invalid numeric value: Value must be a finite number.");

            this.value = value;
            this.unit = unit;
        }

        /// <summary>
        /// Gets the numeric value of this quantity.
        /// </summary>
        public double Value => value;

        /// <summary>
        /// Gets the unit of this quantity.
        /// </summary>
        public U Unit => unit;

        /// <summary>
        /// Defines supported arithmetic operations for validation.
        /// Used internally to map operations to validation logic.
        /// </summary>
        private enum ArithmeticOperation
        {
            ADD,
            SUBTRACT,
            DIVIDE
        }

        // -----------------------------------------------------------------------------------------
        // INTERNAL BASE CONVERSION (Reflection based)
        // -----------------------------------------------------------------------------------------
        
        /// <summary>
        /// Converts the current quantity value to its category's base unit.
        /// Why: To normalize all quantities to a common base for comparison and arithmetic.
        /// How: Uses reflection to locate the appropriate Extension class (e.g., LengthUnitExtensions) and its ConvertToBaseUnit method.
        /// </summary>
        /// <returns>The value in the base unit.</returns>
        private double ConvertToBase()
        {
            var unitType = unit.GetType();
            // Naming convention: {UnitEnum}Extensions (e.g., LengthUnitExtensions)
            var extensionType = unitType.Assembly.GetType($"{unitType.Namespace}.{unitType.Name}Extensions");

            if (extensionType == null)
                throw new NotSupportedException($"No extension class found for unit type {unitType.Name}. Expected {unitType.Name}Extensions.");

            // Dynamically invoke the static extension method.
            var method = extensionType.GetMethod(
                "ConvertToBaseUnit",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            if (method == null)
                throw new NotSupportedException($"'ConvertToBaseUnit' method missing in {extensionType.Name}.");

            return (double)method.Invoke(null, new object[] { unit, value })!;
        }

        /// <summary>
        /// Converts a normalized base value to the target unit.
        /// Why: To present the results of arithmetic or conversion back in specific units.
        /// </summary>
        /// <param name="baseValue">The value in the base unit.</param>
        /// <param name="targetUnit">The target unit to convert to.</param>
        /// <returns>The value in the target unit.</returns>
        private double ConvertFromBase(double baseValue, U targetUnit)
        {
            var unitType = targetUnit.GetType();
            var extensionType = unitType.Assembly.GetType($"{unitType.Namespace}.{unitType.Name}Extensions");

            if (extensionType == null)
                throw new NotSupportedException($"No extension class found for unit type {unitType.Name}. Expected {unitType.Name}Extensions.");

            var method = extensionType.GetMethod(
                "ConvertFromBaseUnit",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            if (method == null)
                throw new NotSupportedException($"'ConvertFromBaseUnit' method missing in {extensionType.Name}.");

            return (double)method.Invoke(null, new object[] { targetUnit, baseValue })!;
        }

        /// <summary>
        /// Validates whether a specific arithmetic operation is supported for the current unit category.
        /// For example, Temperature does not support addition because absolute temperature addition is physically incorrect.
        /// </summary>
        private void ValidateOperationSupport(string operation)
        {
            var unitType = unit.GetType();
            var extensionType = unitType.Assembly.GetType($"{unitType.Namespace}.{unitType.Name}Extensions");

            if (extensionType == null) return;

            // Check if the extension class has a custom ValidateOperationSupport method.
            var method = extensionType.GetMethod(
                "ValidateOperationSupport",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            if (method != null)
            {
                try
                {
                    method.Invoke(null, new object[] { unit, operation });
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    // Unwrap the inner exception (e.g., NotSupportedException from the extension method) to keep stack trace clean.
                    throw ex.InnerException ?? ex;
                }
            }
        }

        /// <summary>
        /// Validates that two quantities can be used together in an arithmetic operation.
        /// They must belong to the same unit category (e.g., you cannot add Weight to Length).
        /// </summary>
        private void ValidateArithmeticOperands(Quantity<U> other)
        {
            if (other == null)
                throw new ArgumentException("Other quantity cannot be null");

            // Strict check on the underlying enum type to prevent mixing categories.
            if (!unit.GetType().Equals(other.unit.GetType()))
                throw new ArgumentException($"Measurement category mismatch: Cannot operate on {unit.GetType().Name} and {other.unit.GetType().Name}.");
        }

        /// <summary>
        /// Performs arithmetic by converting both operands to the base unit first.
        /// Why: Normalization ensures that 1 foot + 12 inches is calculated as 2 feet.
        /// </summary>
        private double PerformBaseArithmetic(Quantity<U> other, ArithmeticOperation operation)
        {
            ValidateArithmeticOperands(other);
            ValidateOperationSupport(operation.ToString());

            // Convert both to base unit (e.g., both to Feet)
            double base1 = this.ConvertToBase();
            double base2 = other.ConvertToBase();

            return operation switch
            {
                ArithmeticOperation.ADD => base1 + base2,
                ArithmeticOperation.SUBTRACT => base1 - base2,
                ArithmeticOperation.DIVIDE =>
                    Math.Abs(base2) < EPSILON
                        ? throw new DivideByZeroException("Cannot divide by zero quantity.")
                        : base1 / base2,
                _ => throw new NotSupportedException($"Operation {operation} is not supported.")
            };
        }

        // -----------------------------------------------------------------------------------------
        // PUBLIC CONVERSION AND ARITHMETIC
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Converts this quantity to a different unit of the same category.
        /// How: Normalizes to base unit then scales up/down to the target unit.
        /// </summary>
        public Quantity<U> ConvertTo(U targetUnit)
        {
            double baseValue = ConvertToBase();
            double converted = ConvertFromBase(baseValue, targetUnit);

            // Rounding to 6 decimal places to maintain precision without floating point noise.
            return new Quantity<U>(Math.Round(converted, 6), targetUnit);
        }

        /// <summary>
        /// Adds another quantity to this one, resulting in a new quantity in this unit.
        /// </summary>
        public Quantity<U> Add(Quantity<U> other)
        {
            return Add(other, this.unit);
        }

        /// <summary>
        /// Adds another quantity to this one, resulting in a new quantity in the target unit.
        /// </summary>
        public Quantity<U> Add(Quantity<U> other, U targetUnit)
        {
            double baseResult = PerformBaseArithmetic(other, ArithmeticOperation.ADD);
            double result = ConvertFromBase(baseResult, targetUnit);

            return new Quantity<U>(Math.Round(result, 6), targetUnit);
        }

        /// <summary>
        /// Subtracts another quantity from this one, resulting in a new quantity in this unit.
        /// </summary>
        public Quantity<U> Subtract(Quantity<U> other)
        {
            return Subtract(other, this.unit);
        }

        /// <summary>
        /// Subtracts another quantity from this one, resulting in a new quantity in the target unit.
        /// </summary>
        public Quantity<U> Subtract(Quantity<U> other, U targetUnit)
        {
            double baseResult = PerformBaseArithmetic(other, ArithmeticOperation.SUBTRACT);
            double result = ConvertFromBase(baseResult, targetUnit);

            return new Quantity<U>(Math.Round(result, 6), targetUnit);
        }

        /// <summary>
        /// Divides this quantity by another, resulting in a dimensionless ratio.
        /// Why: To find how many times one quantity fits into another (e.g., 10 feet / 2 feet = 5).
        /// </summary>
        public double Divide(Quantity<U> other)
        {
            return PerformBaseArithmetic(other, ArithmeticOperation.DIVIDE);
        }

        // -----------------------------------------------------------------------------------------
        // EQUALITY AND FORMATTING
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Determines whether this quantity is equal to another object.
        /// Why: Quantities are equal if they represent the same magnitude in the same category (e.g., 1 foot == 12 inches).
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj == null || obj.GetType() != this.GetType()) return false;

            var other = (Quantity<U>)obj;

            // Ensure categories match (prevent comparing Length to Weight)
            if (!unit.GetType().Equals(other.unit.GetType()))
                return false;

            // Compare values after converting to base unit for consistency across units.
            return Math.Abs(this.ConvertToBase() - other.ConvertToBase()) < EPSILON;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        public override int GetHashCode()
        {
            // Hash the base value to ensure 1 foot and 12 inches produce the same hash code.
            return ConvertToBase().GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current quantity.
        /// </summary>
        public override string ToString()
        {
            return $"{value} {unit}";
        }
    }
}
