using Microsoft.Extensions.Options;
using System.Linq;
using TTB.BankAccountConsent.Configurations;

namespace TTB.BankAccountConsent.Services.Base
{
    public class ConfigSftpBase
    {
        private readonly IOptions<sFtpPath> options;

        public ConfigSftpBase(IOptions<sFtpPath> options)
        {
            this.options = options;
        }

        /// <summary>
        /// Get config by company id
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public ConfigDetail ConfigSftpByCompany(string companyId)
        {
            //Create an instance of the ConfigDetail class.
            var opt = options.Value.ConfigDetail.Where(o => o.CompId.Equals(companyId.Trim())).FirstOrDefault();
            var config = new ConfigDetail()
            {
                Server = opt?.Server,
                Port = opt?.Port,
                Username = opt?.Username,
                Password = opt?.Password,
                Input = opt?.Input,
                Output = opt?.Output,
                KeyFilePath = opt?.KeyFilePath,
                KeyFileName = opt?.KeyFileName,
            };

            return config;
        }
    }
}
