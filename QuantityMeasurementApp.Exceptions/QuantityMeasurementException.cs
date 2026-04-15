using System;

namespace QuantityMeasurementApp.Exceptions
{
    /// <summary>
    /// Custom exception for handling specific errors during quantity measurement operations.
    /// Provides detailed information about logical errors, unit mismatches, or invalid operations.
    /// Why: To provide a specific exception type that can be caught and handled separately from general exceptions.
    /// How: Inherits from the base Exception class and accepts a descriptive error message.
    /// </summary>
    public class QuantityMeasurementException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityMeasurementException"/> class with a specific message.
        /// </summary>
        /// <param name="message">The error message describing the reason for the exception.</param>
        public QuantityMeasurementException(string message) : base(message)
        {
            // Passes the error message to the base Exception class for standard reporting.
        }
    }
}
