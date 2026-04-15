using System;

namespace QuantityMeasurementApp.Exceptions
{
    /// <summary>
    /// Represents errors that occur during database or persistence operations
    /// within the Quantity Measurement Application.
    /// This exception is used to wrap lower-level data access errors.
    /// </summary>
    public class DatabaseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DatabaseException(string message) : base(message)
        {
            // Initializes the base Exception with the provided message.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public DatabaseException(string message, Exception innerException)
            : base(message, innerException)
        {
            // Wraps an existing exception with a more specific database-related context.
        }
    }
}
