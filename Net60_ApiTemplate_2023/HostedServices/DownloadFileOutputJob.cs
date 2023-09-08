using Quartz;
using Serilog;

namespace TTB.BankAccountConsent.Services.BankAccountConsentService
{
    [DisallowConcurrentExecution]
    public class DownloadFileJob : IJob
    {
        private readonly IBankAccountConsentService _service;

        public DownloadFileJob(IBankAccountConsentService service)
        {
            _service = service;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Log.Information("[TTBDownloadFile]  - Start {datetime}", DateTime.Now);

                Log.Information("[TTBDownloadFile]  - Download File...");
                await _service.DownloadFile();

                Log.Information("[TTBDownloadFile]  - Done! {datetime}", DateTime.Now);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[TTBDownloadFile]  - An error occurred.");
            }
        }
    }
}