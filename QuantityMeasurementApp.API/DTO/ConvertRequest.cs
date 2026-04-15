namespace QuantityMeasurementApp.API.DTO 
{   
    /// <summary>
    /// Data Transfer Object for handling conversion requests.
    /// Why: Encapsulates the source value, unit, and the desired target unit into a single object for HTTP POST bodies.
    /// How: Used by the Convert endpoint to parse JSON requests.
    /// </summary>
    public class ConvertRequest 
    { 
        /// <summary>
        /// Gets or sets the numeric value of the quantity to convert.
        /// </summary>
        public double Value { get; set; } 

        /// <summary>
        /// Gets or sets the string representation of the source unit (e.g., "FEET").
        /// </summary>
        public required string Unit { get; set; } 

        /// <summary>
        /// Gets or sets the string representation of the target unit (e.g., "INCH").
        /// </summary>
        public required string TargetUnit { get; set; } 
    } 
}
