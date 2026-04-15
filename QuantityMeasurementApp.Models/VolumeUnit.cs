using System;

namespace QuantityMeasurementApp.Models
{
    /// <summary>
    /// Units of volume measurement supported by the system.
    /// Why: To provide a restricted set of valid units for volume calculations.
    /// </summary>
    public enum VolumeUnit
    {
        Litre,
        Millilitre,
        Gallon
    }

    /// <summary>
    /// Provides conversion logic for Volume units relative to the base unit (Litre).
    /// Why: To centralize volume unit conversion logic.
    /// How: Maps each unit to its specific conversion factor relative to Litres.
    /// </summary>
    public static class VolumeUnitExtensions
    {
        /// <summary>
        /// Returns the conversion factor to convert this unit to the base unit (Litre).
        /// Why: This factor is the ratio between the unit and Litres.
        /// </summary>
        public static double GetConversionFactor(this VolumeUnit unit)
        {
            return unit switch
            {
                VolumeUnit.Litre => 1.0,         // 1.0 Litre = 1.0 Litre (Base)
                VolumeUnit.Millilitre => 0.001,  // 1000.0 ml = 1.0 litre
                VolumeUnit.Gallon => 3.78541,    // 1.0 US gallon = 3.78541 litres
                _ => throw new ArgumentException("Invalid Volume Unit")
            };
        }

        /// <summary>
        /// Converts a value from this unit to the base unit (Litre).
        /// </summary>
        public static double ConvertToBaseUnit(this VolumeUnit unit, double value)
        {
            return value * unit.GetConversionFactor();
        }

        /// <summary>
        /// Converts a value from the base unit (Litre) to this unit.
        /// </summary>
        public static double ConvertFromBaseUnit(this VolumeUnit unit, double baseValue)
        {
            return baseValue / unit.GetConversionFactor();
        }

        /// <summary>
        /// Returns a uppercase string representation of the unit name.
        /// </summary>
        public static string GetUnitName(this VolumeUnit unit)
        {
            return unit.ToString().ToUpper();
        }
    }
}
