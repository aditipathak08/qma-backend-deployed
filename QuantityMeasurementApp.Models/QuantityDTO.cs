using System.Diagnostics.CodeAnalysis;

namespace QuantityMeasurementApp.Models
{
    /// <summary>
    /// Data Transfer Object for representing a quantity in a serializable format.
    /// Why: To pass quantity data between the UI, Controller, and Service without exposing the full generic logic.
    /// How: Uses simple primitive types (double, string) that are easily serialized to JSON or stored in a database.
    /// </summary>
    public class QuantityDTO
    {
        /// <summary>
        /// Gets or sets the numeric value of the quantity.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the string representation of the unit (e.g., "FEET", "INCH").
        /// </summary>
        public required string Unit { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityDTO"/> class.
        /// Why: Required for serialization/deserialization frameworks.
        /// </summary>
        public QuantityDTO() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityDTO"/> class with value and unit.
        /// Why: Convenient constructor for creating DTOs from existing data.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <param name="unit">The unit name.</param>
        [SetsRequiredMembers]
        public QuantityDTO(double value, string unit)
        {
            Value = value;
            Unit = unit;
        }
        /// <summary>
        /// Returns a string representation of the quantity for display or persistence.
        /// Why: To provide a human-readable result in reports and databases.
        /// </summary>
        /// <returns>A string in the format "Value Unit".</returns>
        public override string ToString()
        {
            return $"{Value} {Unit}";
        }
    }
}
