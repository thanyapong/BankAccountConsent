using Microsoft.Extensions.Options;
using PgpCore;
using Serilog;
using TTB.BankAccountConsent.Configurations;

namespace TTB.BankAccountConsent.Services.Crypto
{
    public class CryptoServices : ICryptoServices
    {
        private readonly IOptions<PGPSetting> _optPGP;

        public CryptoServices(IOptions<PGPSetting> optPGP)
        {
            _optPGP = optPGP;
        }

        public async Task<FileInfo> EncryptFile(FileInfo inputFile, string pathFileEncrypted)
        {
            if (!Directory.Exists(pathFileEncrypted))
                Directory.CreateDirectory(pathFileEncrypted);

            // Load keys
            Log.Information("[EncryptFile] - Get publicKey path:{path}", _optPGP.Value.EncryptPublicKeyPath);
            FileInfo publicKey = new FileInfo(_optPGP.Value.EncryptPublicKeyPath);
            EncryptionKeys encryptionKeys = new EncryptionKeys(publicKey);
            
            // Reference input/output files
            FileInfo encryptedFile = new FileInfo(pathFileEncrypted + "\\" + inputFile.Name + ".pgp");

            // Encrypt
            Log.Information("[EncryptFile] - Create file Encrypt PGP. path:{path}", encryptedFile);
            using (PGP pgp = new PGP(encryptionKeys))
            {
                await pgp.EncryptFileAsync(inputFile, encryptedFile);
            }
            return encryptedFile;
        }

        public async Task<FileInfo> DecryptFile(FileInfo inputFile, string pathFileDecrypted)
        {
            if (!Directory.Exists(pathFileDecrypted))
                Directory.CreateDirectory(pathFileDecrypted);

            // Load keys
            Log.Information("[DecryptFile] - Get publicKey path:{path}", _optPGP.Value.DecryptPrivateKeyPath);

            FileInfo publicKey = new FileInfo(_optPGP.Value.DecryptPrivateKeyPath);
            EncryptionKeys encryptionKeys = new EncryptionKeys(publicKey, "siamsmile");

            string decryptedFilePath = pathFileDecrypted + "\\" + inputFile.Name.Replace(".pgp", "");

            if (File.Exists(decryptedFilePath))
            {
                File.Delete(decryptedFilePath);
                Log.Information("[DecryptFile] - Delete file success: {decryptedFilePath}", decryptedFilePath);
            }

            FileInfo decryptedFile = new FileInfo(decryptedFilePath);

            // Encrypt
            Log.Information("[DecryptFile] - Create file Decrypt PGP. path:{path}", decryptedFile);
            using (PGP pgp = new PGP(encryptionKeys))
            {
                await pgp.DecryptFileAsync(inputFile, decryptedFile);
            }

            return decryptedFile;
        }
    }
}
