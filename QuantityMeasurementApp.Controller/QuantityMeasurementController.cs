using System;
using QuantityMeasurementApp.Models;
using QuantityMeasurementApp.Service;

namespace QuantityMeasurementApp.Controller
{
    /// <summary>
    /// Controller for the Quantity Measurement Application.
    /// Orchestrates interaction between the user interface and the service layer.
    /// Why: To decouple the UI logic from the business logic, following the MVC/layered pattern.
    /// How: Receives input from the UI, delegates processing to IQuantityMeasurementService, and handles output/formatting.
    /// </summary>
    public class QuantityMeasurementController
    {
        private readonly IQuantityMeasurementService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantityMeasurementController"/> class.
        /// </summary>
        /// <param name="service">The measurement service implementation to be used.</param>
        public QuantityMeasurementController(IQuantityMeasurementService service)
        {
            // Dependency injection ensures the controller can work with any service implementation.
            this.service = service;
        }

        /// <summary>
        /// Performs a comparison between two quantities and prints the result to the console.
        /// Why: To allow users to verify if two quantities of different units are equal (e.g., 1 ft vs 12 in).
        /// </summary>
        /// <param name="q1">The first quantity DTO.</param>
        /// <param name="q2">The second quantity DTO.</param>
        public void PerformComparison(QuantityDTO q1, QuantityDTO q2)
        {
            try
            {
                // Delegate the comparison logic to the service.
                var result = service.Compare(q1, q2);
                Console.WriteLine($"[RESULT] Comparison: {q1.Value} {q1.Unit} == {q2.Value} {q2.Unit} -> {result}");
            }
            catch (Exception ex)
            {
                // Capture and display any errors (e.g., unit mismatch, invalid values).
                // If it's a database error, we show the inner message to help with connectivity diagnostics.
                string message = ex.InnerException != null ? $"{ex.Message} (Reason: {ex.InnerException.Message})" : ex.Message;
                Console.WriteLine($"[ERROR] Comparison failed: {message}");
            }
        }

        /// <summary>
        /// Performs a conversion of a quantity to a target unit and prints the result.
        /// Why: To allow users to see a value expressed in a different unit of the same category.
        /// </summary>
        /// <param name="q">The source quantity DTO.</param>
        /// <param name="targetUnit">The name of the unit to convert to.</param>
        public void PerformConversion(QuantityDTO q, string targetUnit)
        {
            try
            {
                // Delegate conversion to the service layer.
                var result = service.Convert(q, targetUnit);
                Console.WriteLine($"[RESULT] Conversion: {q.Value} {q.Unit} -> {result.Value} {result.Unit}");
            }
            catch (Exception ex)
            {
                string message = ex.InnerException != null ? $"{ex.Message} (Reason: {ex.InnerException.Message})" : ex.Message;
                Console.WriteLine($"[ERROR] Conversion failed: {message}");
            }
        }

        /// <summary>
        /// Performs addition of two quantities and prints the result.
        /// Why: To support composite measurements (e.g., adding inches to feet).
        /// </summary>
        /// <param name="q1">The first quantity.</param>
        /// <param name="q2">The second quantity.</param>
        public void PerformAddition(QuantityDTO q1, QuantityDTO q2)
        {
            try
            {
                // Addition result is returned as a DTO in the unit of the first operand.
                var result = service.Add(q1, q2);
                Console.WriteLine($"[RESULT] Addition: ({q1.Value} {q1.Unit}) + ({q2.Value} {q2.Unit}) = {result.Value} {result.Unit}");
            }
            catch (Exception ex)
            {
                string message = ex.InnerException != null ? $"{ex.Message} (Reason: {ex.InnerException.Message})" : ex.Message;
                Console.WriteLine($"[ERROR] Addition failed: {message}");
            }
        }

        /// <summary>
        /// Performs subtraction of two quantities and prints the result.
        /// Why: To support finding the difference between measurements.
        /// </summary>
        public void PerformSubtraction(QuantityDTO q1, QuantityDTO q2)
        {
            try
            {
                var result = service.Subtract(q1, q2);
                Console.WriteLine($"[RESULT] Subtraction: ({q1.Value} {q1.Unit}) - ({q2.Value} {q2.Unit}) = {result.Value} {result.Unit}");
            }
            catch (Exception ex)
            {
                string message = ex.InnerException != null ? $"{ex.Message} (Reason: {ex.InnerException.Message})" : ex.Message;
                Console.WriteLine($"[ERROR] Subtraction failed: {message}");
            }
        }

        /// <summary>
        /// Performs division of two quantities and prints the resulting ratio.
        /// Why: To calculate ratios between quantities (e.g., how many feet in a yard).
        /// </summary>
        public void PerformDivision(QuantityDTO q1, QuantityDTO q2)
        {
            try
            {
                // Division results in a dimensionless scalar (double).
                var result = service.Divide(q1, q2);
                Console.WriteLine($"[RESULT] Division: ({q1.Value} {q1.Unit}) / ({q2.Value} {q2.Unit}) = {result}");
            }
            catch (Exception ex)
            {
                string message = ex.InnerException != null ? $"{ex.Message} (Reason: {ex.InnerException.Message})" : ex.Message;
                Console.WriteLine($"[ERROR] Division failed: {message}");
            }
        }
    }
}
