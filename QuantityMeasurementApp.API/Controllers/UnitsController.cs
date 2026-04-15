using Microsoft.AspNetCore.Mvc;
using QuantityMeasurementApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantityMeasurementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetUnits()
        {
            var units = new Dictionary<string, List<string>>
            {
                { "Length", Enum.GetNames(typeof(LengthUnit)).ToList() },
                { "Weight", Enum.GetNames(typeof(WeightUnit)).ToList() },
                { "Volume", Enum.GetNames(typeof(VolumeUnit)).ToList() },
                { "Temperature", Enum.GetNames(typeof(TemperatureUnit)).ToList() }
            };

            return Ok(units);
        }
    }
}
