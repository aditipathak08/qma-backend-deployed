using System;

namespace QuantityMeasurementApp.Models
{
    /// <summary>
    /// Units of temperature measurement supported by the system.
    /// Why: To provide a restricted set of valid units for temperature calculations.
    /// </summary>
    public enum TemperatureUnit
    {
        Celsius,
        Fahrenheit,
        Kelvin
    }

    /// <summary>
    /// Provides conversion logic for Temperature units relative to the base unit (Celsius).
    /// Why: Temperature conversion is non-linear (involves offsets), requiring specialized logic.
    /// How: Uses formula-based mapping for each unit to Celsius.
    /// </summary>
    public static class TemperatureUnitExtensions
    {
        /// <summary>
        /// Converts a value from this unit to the base unit (Celsius).
        /// Why: Normalization is required for equality comparison.
        /// </summary>
        public static double ConvertToBaseUnit(this TemperatureUnit unit, double value)
        {
            // Temperature scales use different zero points and step sizes.
            return unit switch
            {
                TemperatureUnit.Celsius => value,
                TemperatureUnit.Fahrenheit => (value - 32) * 5.0 / 9.0, // (F - 32) * 5/9 = C
                TemperatureUnit.Kelvin => value - 273.15,               // K - 273.15 = C
                _ => throw new ArgumentException("Unsupported temperature unit")
            };
        }

        /// <summary>
        /// Converts a value from the base unit (Celsius) to this unit.
        /// Why: To present normalized temperature results in a specific target unit.
        /// </summary>
        public static double ConvertFromBaseUnit(this TemperatureUnit unit, double baseValue)
        {
            return unit switch
            {
                TemperatureUnit.Celsius => baseValue,
                TemperatureUnit.Fahrenheit => (baseValue * 9.0 / 5.0) + 32, // (C * 9/5) + 32 = F
                TemperatureUnit.Kelvin => baseValue + 273.15,               // C + 273.15 = K
                _ => throw new ArgumentException("Unsupported temperature unit")
            };
        }

        /// <summary>
        /// Validates whether a specific arithmetic operation is supported for Temperature.
        /// Why: Adding 10°C to 10°C is physically nonsensical as temperatures represent states, not displacements.
        /// How: Throws NotSupportedException if ADD or SUBTRACT is attempted.
        /// </summary>
        public static void ValidateOperationSupport(this TemperatureUnit unit, string operation)
        {
            if (operation.Equals("ADD", StringComparison.OrdinalIgnoreCase) || 
                operation.Equals("SUBTRACT", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(
                    $"Temperature unit ({unit}) does not support {operation} operations because absolute temperature addition/subtraction is not physically meaningful.");
            }
        }
    }
}
