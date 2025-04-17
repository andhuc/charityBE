using Microsoft.AspNetCore.Mvc;
using NET_base.Models.Common;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {

        private string UPLOAD_FOLDER = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        private string UPLOAD_PATH = "uploads";

        [HttpPost]
        public async Task<Response<string>> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new Response<string>(false, "No file uploaded.", null);

            try
            {
                var fileId = Guid.NewGuid().ToString();
                var fileExtension = Path.GetExtension(file.FileName);
                var newFileName = $"{UPLOAD_PATH}/{fileId}{fileExtension}";
                var filePath = Path.Combine(UPLOAD_FOLDER, newFileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return new Response<string>(true, "File uploaded successfully.", newFileName);
            }
            catch (Exception)
            {
                return new Response<string>(false, "Error uploading file.", null);
            }
        }
    }
}
