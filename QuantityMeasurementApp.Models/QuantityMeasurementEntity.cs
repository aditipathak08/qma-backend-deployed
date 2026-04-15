using System;
using System.Diagnostics.CodeAnalysis;

namespace QuantityMeasurementApp.Models
{
    /// <summary>
    /// Represents a persistent entity for storing measurement operation details.
    /// Why: To keep a history of all operations (CONVERT, ADD, etc.) performed by users.
    /// How: Maps to a database table or a cached object list, storing operands and results.
    /// </summary>
    [Serializable]
    public class QuantityMeasurementEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the first operand of the operation.
        /// </summary>
        public required QuantityDTO Operand1 { get; set; }

        /// <summary>
        /// Gets or sets the second operand (optional, used for ADD, SUBTRACT, etc.).
        /// </summary>
        public QuantityDTO? Operand2 { get; set; }

        /// <summary>
        /// Gets or sets the operation type (e.g., "CONVERT", "ADD").
        /// </summary>
        public required string Operation { get; set; }

        /// <summary>
        /// Gets or sets the calculated result of the operation.
        /// </summary>
        public QuantityDTO? Result { get; set; }

        /// <summary>
        /// Gets or sets any error message that occurred during the operation.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Returns true if the operation resulted in an error.
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityMeasurementEntity"/> class.
        /// Why: Required for serialization frameworks.
        /// </summary>
        public QuantityMeasurementEntity() { }

        /// <summary>
        /// Initializes a new instance with required fields.
        /// Why: Ensures basic operation details are captured upon creation.
        /// </summary>
        /// <param name="operand1">The first operand.</param>
        /// <param name="operation">The operation string.</param>
        [SetsRequiredMembers]
        public QuantityMeasurementEntity(QuantityDTO operand1, string operation)
        {
            Operand1 = operand1;
            Operation = operation;
        }
    }
}
