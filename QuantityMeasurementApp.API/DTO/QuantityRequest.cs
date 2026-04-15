using QuantityMeasurementApp.Models;

namespace QuantityMeasurementApp.API.DTO
{
    public class ComparisonRequest
    {
        public required QuantityDTO First { get; set; }
        public required QuantityDTO Second { get; set; }
    }

    public class ArithmeticRequest
    {
        public required QuantityDTO First { get; set; }
        public required QuantityDTO Second { get; set; }
        public string? TargetUnit { get; set; }
    }
}
