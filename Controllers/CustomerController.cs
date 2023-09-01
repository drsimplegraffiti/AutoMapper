
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Techie.Modal;
using Techie.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Techie.Container
{
    [EnableRateLimiting("fixed window")]
    // [EnableCors("CorsPolicy")]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;
        public CustomerController(ICustomerService service)
        {
            _service = service;
        }

        // [DisableCors]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAll();
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerModel customer)
        {
             await _service.AddCustomer(customer);
            return Ok("Added successfully");
        }

        [DisableRateLimiting]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetById(id);
            if (data == null)
                return NotFound("No data found");
            return Ok(data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveById(int id)
        {
            var data = await _service.RemoveById(id);
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] CustomerModel customer, int id)
        {
            var data = await _service.Update(customer, id);
            if (data == null)
                return NotFound();
            return Ok(data);
        }
       
    }
}

