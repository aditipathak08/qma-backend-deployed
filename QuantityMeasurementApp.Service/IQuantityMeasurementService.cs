using QuantityMeasurementApp.Models;

namespace QuantityMeasurementApp.Service
{
    public interface IQuantityMeasurementService
    {
        bool Compare(QuantityDTO q1, QuantityDTO q2);
        QuantityDTO Convert(QuantityDTO source, string targetUnit);
        QuantityDTO Add(QuantityDTO q1, QuantityDTO q2, string? targetUnit = null);
        QuantityDTO Subtract(QuantityDTO q1, QuantityDTO q2, string? targetUnit = null);
        double Divide(QuantityDTO q1, QuantityDTO q2);
        double Multiply(QuantityDTO q1, QuantityDTO q2); // scalar multiply for simplicity in this context
    }
}
