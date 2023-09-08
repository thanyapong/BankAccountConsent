namespace TTB.BankAccountConsent.Configurations
{
    public class sFtpPath
    {
        public ConfigDetail[] ConfigDetail { get; set; }
    }

    public class ConfigDetail
    {
        public string CompId { get; set; }
        public string Server { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public string KeyFilePath { get; set; }
        public string KeyFileName { get; set; }
    }
}
