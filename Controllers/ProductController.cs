
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Techie.Modal;
using Techie.Repos;
using Techie.Repos.Models;

namespace Techie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IWebHostEnvironment _env; // IWebHostEnvironment is used to get the path of the wwwroot folder
        // the wwwroot folder is used to store the images in the project
        private readonly LearnDataContext _context;
        public ProductController(IWebHostEnvironment env, LearnDataContext context)
        {
            _env = env;
            _context = context;
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


          [HttpPut("DBMultipleUploadImage")]
        public async Task<IActionResult> DBMultipleUploadImage(IFormFileCollection fileCollection, string productcode)
        {
            ApiResponse response = new ApiResponse();
            int passcount = 0; int errorcount = 0;
            try
            {
                foreach (var file in fileCollection)
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        _context.ProductImages.Add(new ProductImage()
                        {
                            Productcode = productcode,
                            Productimage = stream.ToArray()
                        });
                        await _context.SaveChangesAsync();
                        passcount++;
                    }
                }

            }
            catch (Exception ex)
            {
                errorcount++;
                response.ErrorMessage = ex.Message;
            }
            response.ResponseCode = 200;
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
                        if (System.IO.File.Exists(imagePath))
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

        // download image
        [HttpGet("download")]
        public async Task<IActionResult> Download(string productcode)
        {
            // string imageurl = string.Empty;
            // string hosturl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string FilePath = GetFilePath(productcode);
                string imagePath = FilePath + "/" + productcode + ".png";
                if (System.IO.File.Exists(imagePath))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    using (FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    {
                        await fileStream.CopyToAsync(memoryStream);
                    }
                    memoryStream.Position = 0;
                    return File(memoryStream, "image/png", productcode + ".png");
                    // imageurl = hosturl + "/Upload/product/" + productcode + "/" + productcode + ".png";
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
        }

         [HttpGet("DBdownload")]
        public async Task<IActionResult> DBDownload(string productcode)
        {
           
            try
            {
                var _productImage = await _context.ProductImages.Where(x => x.Productcode == productcode).FirstOrDefaultAsync();
                if (_productImage != null)
                {
                    // MemoryStream memoryStream = new MemoryStream();
                    // memoryStream.Write(_productImage.Productimage, 0, _productImage.Productimage.Length);
                    // memoryStream.Position = 0;
                    // return File(memoryStream, "image/png", productcode + ".png");
                    return File(_productImage.Productimage, "image/png", productcode + ".png");
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
        }

         // download image
        [HttpDelete("remove")]
        public async Task<IActionResult> Remove(string productcode)
        {
            // string imageurl = string.Empty;
            // string hosturl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                string FilePath = GetFilePath(productcode);
                string imagePath = FilePath + "/" + productcode + ".png";
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                    return Ok("Image Deleted Successfully");
                    // imageurl = hosturl + "/Upload/product/" + productcode + "/" + productcode + ".png";
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


        }

           // download image
        [HttpDelete("multiremove")]
        public async Task<IActionResult> Multiremove(string productcode)
        {
            // string imageurl = string.Empty;
            // string hosturl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
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
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                    return Ok("Image Deleted Successfully");
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


        }


 [HttpGet("GetDBMultipleImage")]
        public IActionResult GetDBMultipleImage(string productcode)
        {
            List<string> imageurl = new List<string>();
            // string hosturl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            try
            {
                var productImages = _context.ProductImages.Where(x => x.Productcode == productcode).ToList();
                if (productImages.Count > 0 && productImages != null)
                {
                    foreach (var productImage in productImages)
                    {
                        string base64 = Convert.ToBase64String(productImage.Productimage);
                        string _ImageUrl = "data:image/png;base64," + base64;
                        imageurl.Add(_ImageUrl);
                    }
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


        [NonAction] // this is same as [ApiExplorerSettings(IgnoreApi = true)]
        private string GetFilePath(string productcode)
        {
            return Path.Combine(_env.WebRootPath, "Upload", "product", productcode);
        }

    }
}