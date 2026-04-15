using QuantityMeasurementApp.Models;

namespace QuantityMeasurementApp.Repository
{
    public interface IQuantityMeasurementRepository
    {
        void Save(QuantityMeasurementEntity entity);
    }
}
