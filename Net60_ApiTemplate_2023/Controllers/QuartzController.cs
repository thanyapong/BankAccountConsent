using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Serilog;

namespace TTB.BankAccountConsent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuartzController : ControllerBase
    {
        #region Constructor

        private readonly ISchedulerFactory _factory;

        public QuartzController(ISchedulerFactory factory)
        {
            _factory = factory;
        }

        #endregion Constructor

        #region API

        /// <summary>
        /// POST DownloadFileJob
        /// </summary>
        /// <returns></returns>
        [HttpPost("downloadfilejob")]
        public async Task<OkObjectResult> RunDownloadFileJob()
        {
            IScheduler scheduler = await _factory.GetScheduler();
            await scheduler.TriggerJob(new JobKey("DownloadFileJob"));
            Log.Information("[TriggerJob]  - TTBDownloadFile Time:{date}", DateTime.Now);
            return Ok("OK");
        }

        #endregion API
    }
}
