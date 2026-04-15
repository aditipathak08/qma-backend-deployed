namespace QuantityMeasurementApp.Models
{
    /// <summary>
    /// Internal model representing a quantity with a specific enum-based unit.
    /// Why: Provides a typed structure for quantities within the application layers.
    /// How: Uses a generic type U constrained to Enum for unit safety.
    /// </summary>
    /// <typeparam name="U">The unit enum type.</typeparam>
    public class QuantityModel<U> where U : System.Enum
    {
        /// <summary>
        /// Gets or sets the numeric value of the quantity.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the typed unit.
        /// </summary>
        public U Unit { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityModel{U}"/> class.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <param name="unit">The unit.</param>
        public QuantityModel(double value, U unit)
        {
            Value = value;
            Unit = unit;
        }
    }
}

