namespace TTB.BankAccountConsent.Services.Crypto
{
    public interface ICryptoServices
    {
        Task<FileInfo> DecryptFile(FileInfo inputFile, string pathFileDecrypted);
        Task<FileInfo> EncryptFile(FileInfo inputFile, string pathFileEncrypted);
    }
}
