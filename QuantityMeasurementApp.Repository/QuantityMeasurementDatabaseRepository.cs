using QuantityMeasurementApp.Models;
using QuantityMeasurementApp.Exceptions;
using System;

namespace QuantityMeasurementApp.Repository
{
    public class QuantityMeasurementDatabaseRepository : IQuantityMeasurementRepository
    {
        private readonly QuantityMeasurementDbContext _context;

        public QuantityMeasurementDatabaseRepository(QuantityMeasurementDbContext context)
        {
            _context = context;
        }

        public void Save(QuantityMeasurementEntity entity)
        {
            try
            {
                _context.QuantityMeasurements.Add(entity);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Database error while saving measurement using EF Core", ex);
            }
        }
    }
}
