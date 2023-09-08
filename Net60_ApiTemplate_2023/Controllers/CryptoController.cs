using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TTB.BankAccountConsent.Services.Crypto;

namespace TTB.BankAccountConsent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CryptoController : ControllerBase
    {
        private readonly ICryptoServices _services;

        public CryptoController(ICryptoServices services)
        {
            _services = services;
        }

        [HttpPost("encryptfile")]
        public async Task<IActionResult> EncryptFile([Required(ErrorMessage = "Please select a file.")] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Ok("Invalid file or No data in file.");
            }

            var inputFilePath = "C:\\TTBFile\\InputFile";
            if (!Directory.Exists(inputFilePath))
                Directory.CreateDirectory(inputFilePath);

            var filePath = Path.Combine(inputFilePath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            FileInfo fileInfo = new FileInfo(filePath);

            var endcryptFilePath = "C:\\TTBFile\\EndCryptFile";
            if (!Directory.Exists(inputFilePath))
                Directory.CreateDirectory(inputFilePath);

            var encryptFile = await _services.EncryptFile(fileInfo, endcryptFilePath);

            string contentType = "application/pgp";

            return File(System.IO.File.OpenRead(encryptFile.FullName), contentType, encryptFile.Name);
        }

        [HttpPost("decryptfile")]
        public async Task<IActionResult> DecryptFile([Required(ErrorMessage = "Please select a file.")] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Ok("Invalid file or No data in file.");
            }

            var inputFilePath = "C:\\TTBFile\\InputFile";
            if (!Directory.Exists(inputFilePath))
                Directory.CreateDirectory(inputFilePath);

            var filePath = Path.Combine(inputFilePath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            FileInfo fileInfo = new FileInfo(filePath);

            var decryptFilePath = "C:\\TTBFile\\DecryptFile";
            if (!Directory.Exists(inputFilePath))
                Directory.CreateDirectory(inputFilePath);

            var encryptFile = await _services.DecryptFile(fileInfo, decryptFilePath);

            string contentType = "application/pgp";

            return File(System.IO.File.OpenRead(encryptFile.FullName), contentType, encryptFile.Name);
        }
    }
}
