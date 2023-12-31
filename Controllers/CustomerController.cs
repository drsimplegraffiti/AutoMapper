﻿
using System.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Techie.Modal;
using Techie.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Techie.Container
{


    [Authorize]
    [EnableRateLimiting("fixed window")]
    // [EnableCors("CorsPolicy")]
    [Route("api/[controller]")] // api/customer
    public class CustomerController : ControllerBase
    {
        private readonly IWebHostEnvironment _env; // IWebHostEnvironment is used to get the path of the wwwroot folder

        private readonly ICustomerService _service;
        public CustomerController(ICustomerService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
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

        [AllowAnonymous]
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

        // export to excel
        [AllowAnonymous]
        [HttpGet("export")]
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                string FilePath = GetFilePath();
                string excelpath = Path.Combine(FilePath, "CustomerInfo.xlsx");
                DataTable dt = new DataTable();
                dt.Columns.Add("Code", typeof(string));
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("Email", typeof(string));
                dt.Columns.Add("PhoneNumber", typeof(string));
                dt.Columns.Add("CreditLimit", typeof(int));
                var data = await _service.GetAll();
                if (data != null)
                {

                    foreach (var item in data)
                    {
                        dt.Rows.Add(item.Code, item.Name, item.Email, item.PhoneNumber, item.Creditlimit);
                    }
                }
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.AddWorksheet(dt, "Customer Info");
                    
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        if (System.IO.File.Exists(excelpath))
                        {
                            System.IO.File.Delete(excelpath);
                        }
                        wb.SaveAs(excelpath);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CustomerInfo.xlsx");
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        [NonAction] // this is same as [ApiExplorerSettings(IgnoreApi = true)]
        private string GetFilePath()
        {
            return Path.Combine(_env.WebRootPath, "Export");
        }


    }
}

