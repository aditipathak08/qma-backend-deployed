using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QuantityMeasurementApp.Models;
using QuantityMeasurementApp.Service;
using QuantityMeasurementApp.API.DTO;

namespace QuantityMeasurementApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuantityController : ControllerBase
    {
        private readonly IQuantityMeasurementService service;

        public QuantityController(IQuantityMeasurementService service)
        {
            this.service = service;
        }

        [HttpPost("compare")]
        public IActionResult Compare([FromBody] ComparisonRequest request)
        {
            if (request == null || request.First == null || request.Second == null)
                return BadRequest("Both 'First' and 'Second' quantities must be provided.");

            var isEqual = service.Compare(request.First, request.Second);
            
            return Ok(new 
            { 
                IsEqual = isEqual,
                Message = isEqual ? "The quantities are equal." : "The quantities are NOT equal."
            });
        }

        [HttpPost("convert")]
        public IActionResult Convert([FromBody] ConvertRequest request)
        {
            if (request == null) return BadRequest("Invalid request.");

            var source = new QuantityDTO { Value = request.Value, Unit = request.Unit };
            var result = service.Convert(source, request.TargetUnit);

            return Ok(result);
        }

        [HttpPost("add")]
        public IActionResult Add([FromBody] ArithmeticRequest request)
        {
            if (request == null || request.First == null || request.Second == null)
                return BadRequest("Addition requires 'First' and 'Second' quantities.");

            var result = service.Add(request.First, request.Second, request.TargetUnit);
            return Ok(result);
        }

        [HttpPost("subtract")]
        public IActionResult Subtract([FromBody] ArithmeticRequest request)
        {
            if (request == null || request.First == null || request.Second == null)
                return BadRequest("Subtraction requires 'First' and 'Second' quantities.");

            var result = service.Subtract(request.First, request.Second, request.TargetUnit);
            return Ok(result);
        }

        [HttpPost("divide")]
        public IActionResult Divide([FromBody] ArithmeticRequest request)
        {
            if (request == null || request.First == null || request.Second == null)
                return BadRequest("Division requires 'First' and 'Second' quantities.");

            var result = service.Divide(request.First, request.Second);
            return Ok(new { Ratio = result });
        }

        [HttpPost("multiply")]
        public IActionResult Multiply([FromBody] ArithmeticRequest request)
        {
            if (request == null || request.First == null || request.Second == null)
                return BadRequest("Multiplication requires 'First' and 'Second' quantities.");

            var result = service.Multiply(request.First, request.Second);
            return Ok(new { ProductOfBaseValues = result });
        }
    }
}
