using System;

namespace QuantityMeasurementApp.Models
{
    /// <summary>
    /// Units of length measurement supported by the system.
    /// Why: To provide a restricted set of valid units for length calculations.
    /// How: Defined as an enumeration with specific length types.
    /// </summary>
    public enum LengthUnit
    {
        Feet,
        Inch,
        Yards,
        Centimeters,
        Meter
    }

    /// <summary>
    /// Provides conversion logic for Length units relative to the base unit (Feet).
    /// Why: To centralize unit conversion logic using extension methods, keeping the enum clean.
    /// How: Uses a switch expression to map each unit to its specific conversion factor relative to Feet.
    /// </summary>
    public static class LengthUnitExtensions
    {
        /// <summary>
        /// Returns the conversion factor to convert this unit to the base unit (Feet).
        /// Why: This factor is the ratio between the unit and Feet (e.g., 1 inch = 1/12 foot).
        /// </summary>
        public static double GetConversionFactor(this LengthUnit unit)
        {
            return unit switch
            {
                LengthUnit.Feet => 1.0,              // 1.0 Feet = 1.0 Feet (Base)
                LengthUnit.Inch => 1.0 / 12.0,       // 12.0 inches = 1.0 foot
                LengthUnit.Yards => 3.0,             // 1.0 yard = 3.0 feet
                LengthUnit.Centimeters => 1.0 / 30.48, // 30.48 cm = 1.0 foot
                LengthUnit.Meter => 1.0 / 0.3048,   // 0.3048 meters = 1.0 foot
                _ => throw new ArgumentException("Unsupported Length Unit")
            };
        }

        /// <summary>
        /// Converts a value from this unit to the base unit (Feet).
        /// How: Multiply the input value by the unit's conversion factor.
        /// </summary>
        public static double ConvertToBaseUnit(this LengthUnit unit, double value)
        {
            // Multiplying the value by its scale factor results in the Feet equivalent.
            return value * unit.GetConversionFactor();
        }

        /// <summary>
        /// Converts a value from the base unit (Feet) to this unit.
        /// How: Divide the base value by the unit's conversion factor.
        /// </summary>
        public static double ConvertFromBaseUnit(this LengthUnit unit, double baseValue)
        {
            // Reverse of ConvertToBaseUnit: divide to scale down from the base unit.
            return baseValue / unit.GetConversionFactor();
        }

        /// <summary>
        /// Returns a uppercase string representation of the unit name.
        /// Why: For consistent presentation in the UI/Logs.
        /// </summary>
        public static string GetUnitName(this LengthUnit unit)
        {
            return unit.ToString().ToUpper();
        }
    }
}

