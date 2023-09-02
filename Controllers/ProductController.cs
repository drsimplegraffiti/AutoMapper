
using Microsoft.AspNetCore.Mvc;
using Techie.Modal;

namespace Techie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IWebHostEnvironment _env; // IWebHostEnvironment is used to get the path of the wwwroot folder
        // the wwwroot folder is used to store the images in the project
        public ProductController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPut("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile formFile, string productcode)
        {
            ApiResponse response = new ApiResponse();
            try
            {
                // string filePath   = Path.Combine(_env.WebRootPath, "Upload", "product", formFile.FileName);
                string filePath = GetFilePath(productcode);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                string imagePath = filePath + "/" + productcode + ".png";
                // check if the file exists
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                // save and store the image
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                    response.Result = "Image Uploaded Successfully";
                    response.ResponseCode = 200;
                }
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
            }
            return Ok(response);
        }

        [HttpPut("MultipleUploadImage")]
        public async Task<IActionResult> MultipleUploadImage(IFormFileCollection fileCollection, string productcode)
        {
            ApiResponse response = new ApiResponse();
            int passcount = 0; int errorcount = 0;
            try
            {
                // string filePath   = Path.Combine(_env.WebRootPath, "Upload", "product", formFile.FileName);
                string filePath = GetFilePath(productcode);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                foreach (var file in fileCollection)
                {
                    string imagePath = filePath + "/" + file.FileName;
                    // check if the file exists
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    // save and store the image
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        passcount++;
                    }
                }

            }
            catch (Exception ex)
            {
                errorcount++;
                response.ErrorMessage = ex.Message;
            }
            response.Result = $"{passcount} Image(s) Uploaded Successfully and {errorcount} Image(s) Failed";
            return Ok(response);
        }

        [HttpGet("GetImage")]
        public IActionResult GetImage(string productcode)
        {
            string imageurl = string.Empty;
            string hosturl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string FilePath = GetFilePath(productcode);
                string imagePath = FilePath + "/" + productcode + ".png";
                if (System.IO.File.Exists(imagePath))
                {
                    imageurl = hosturl + "/Upload/product/" + productcode + "/" + productcode + ".png";
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(imageurl);
        }

        [HttpGet("GetMultipleImage")]
        public IActionResult GetMultipleImage(string productcode)
        {
            List<string> imageurl = new List<string>();
            string hosturl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string FilePath = GetFilePath(productcode);
                if (System.IO.Directory.Exists(FilePath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(FilePath);
                    FileInfo[] fileInfos = directoryInfo.GetFiles();
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        string filename = fileInfo.Name;
                        string imagePath = FilePath + "/" + filename;
                        if(System.IO.File.Exists(imagePath))
                        {
                            string _ImageUrl = hosturl + "/Upload/product/" + productcode + "/" + filename;
                            imageurl.Add(_ImageUrl);
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(imageurl);
        }


        [NonAction] // this is same as [ApiExplorerSettings(IgnoreApi = true)]
        private string GetFilePath(string productcode)
        {
            return Path.Combine(_env.WebRootPath, "Upload", "product", productcode);
        }

    }
}