namespace QuantityMeasurementApp.Models
{
    /// <summary>
    /// Interface defining the contract for any measurable quantity category.
    /// Provides necessary methods for unit conversion and arithmetic validation.
    /// Why: To ensure all unit types (Length, Weight, etc.) expose a consistent API for the generic Quantity class.
    /// How: Implementers provide conversion factors and methods to normalize values to a base unit.
    /// </summary>
    public interface IMeasurable
    {
        // -----------------------------------------------------------------------------------------
        // Conversion Methods (Mandatory)
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the conversion factor of the unit relative to its category's base unit.
        /// </summary>
        /// <returns>The factor used to scale the value to the base unit.</returns>
        double GetConversionFactor();                  // Relative to base unit

        /// <summary>
        /// Converts a given value from the implementing unit to the category's base unit.
        /// </summary>
        /// <param name="value">The value in the current unit.</param>
        /// <returns>The normalized value in the base unit.</returns>
        double ConvertToBaseUnit(double value);        // Convert to base unit

        /// <summary>
        /// Converts a value from the base unit back to the implementing unit.
        /// </summary>
        /// <param name="baseValue">The value in the base unit.</param>
        /// <returns>The scaled value in the current unit.</returns>
        double ConvertFromBaseUnit(double baseValue);  // Convert from base unit

        /// <summary>
        /// Returns the human-readable name of the unit.
        /// </summary>
        /// <returns>A string representing the unit.</returns>
        string GetUnitName();                          // Readable unit name

        // -----------------------------------------------------------------------------------------
        // Arithmetic Capability (Default Support)
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Delegate for defining whether a unit category supports basic arithmetic.
        /// </summary>
        public delegate bool SupportsArithmetic();

        /// <summary>
        /// Default implementation: most units support arithmetic operations.
        /// </summary>
        SupportsArithmetic supportsArithmetic => () => true;

        /// <summary>
        /// Checks whether arithmetic operations (ADD, SUBTRACT, DIVIDE) are allowed for this category.
        /// </summary>
        /// <returns>True if arithmetic is supported, otherwise false.</returns>
        public virtual bool SupportsArithmeticOperations()
        {
            // By default, we invoke the delegate which returns true.
            return supportsArithmetic();
        }

        // -----------------------------------------------------------------------------------------
        // Validate Arithmetic Operation (Optional Override)
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Validates if a specific arithmetic operation is allowed.
        /// Units like Temperature will override this to prevent nonsensical additions.
        /// </summary>
        /// <param name="operation">The operation name (e.g., "ADD").</param>
        public virtual void ValidateOperationSupport(string operation)
        {
            // Default behavior: allow operation. No exception is thrown.
        }
    }
}
