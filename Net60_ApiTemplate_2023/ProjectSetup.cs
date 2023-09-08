using TTB.BankAccountConsent.Configurations;
using TTB.BankAccountConsent.Data;
using TTB.BankAccountConsent.HostedServices;
using TTB.BankAccountConsent.Services;
using TTB.BankAccountConsent.Services.Auth;
using TTB.BankAccountConsent.Startups;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Quartz;
using TTB.BankAccountConsent.Services.BankAccountConsentService;
using TTB.BankAccountConsent.Services.Crypto;

namespace TTB.BankAccountConsent
{
    public static class ProjectSetup
    {
        /// <summary>
        /// ใส่ Dependency Injection ที่ใช้ใน Project
        /// </summary>
        public static IServiceCollection ConfigDependency(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // ใช้สำหรับข้อมูล Login
            services.Configure<LocalPath>(configuration.GetSection("LocalPath"));
            services.Configure<sFtpPath>(configuration.GetSection("sFtpPath"));
            services.Configure<Company>(configuration.GetSection("Company"));
            services.Configure<PGPSetting>(configuration.GetSection("PGPSetting"));

            services.AddScoped<ILoginDetailServices, LoginDetailServices>();
            services.AddScoped<IBankAccountConsentService, BankAccountConsentService>();
            services.AddScoped<ICryptoServices, CryptoServices>();
            // TODO: เมื่อเขียน Service และ Interface ของ Service ให้ใส่ที่นี้
            // services.AddSingleton // ใช้เมื่อใช้ Instance เดียวทั้ง Project
            // services.AddScoped // ใช้เมื่อแยก Instance ตาม User
            // services.AddTransient // ใช้เมื่อสร้าง Instance ใหม่ทุกครั้งที่เรียกใช้
            // services.AddScoped<IProductService, ProductService>();


            // TODO: ตัวอย่างการเขียน RestSharp หากไม่ใช้ให้ลบ Folder Examples ทิ้ง
            // วิธีการเขียน RestSharp
            // https://github.com/SiamsmileDev/DevKnowledgeBase/blob/develop/Example%20Code/CSharp/RestSharp%20Example.md

            // services.Configure<ServiceURL>(configuration.GetSection("ServiceURL"));
            // services.AddSingleton<ShortLinkClient>();
            // services.AddSingleton<SendSmsClient>();

            return services;
        }

        /// <summary>
        /// ใส่ Job Schedule ที่ใช้ใน Project
        /// </summary>
        public static IServiceCollectionQuartzConfigurator ConfigQuartz(this IServiceCollectionQuartzConfigurator q, QuartzSetting quartzSetting)
        {
            // TODO: เมื่อเขียน Job Schedule แล้วให้ใส่งานที่นี้ ให้เพิ่ม Schedule ใน appsetting.json ด้วย
            // การสร้าง Job Schedule ดูได้ที่
            // https://github.com/SiamsmileDev/DevKnowledgeBase/blob/develop/Example%20Code/CSharp/Quartz%20Job%20Scheduling.md

            // Job สำหรับการลบ Log ใน Database
            q.AddJobAndTrigger<LoggerRetentionJob>(quartzSetting);

            q.AddJobAndTrigger<DownloadFileJob>(quartzSetting);

            return q;
        }

        /// <summary>
        /// ใส่ Consumer RabbitMQ ที่ใช้ใน Project
        /// </summary>
        public static IServiceCollectionBusConfigurator ConfigRabbitMQ(this IServiceCollectionBusConfigurator configure)
        {
            // TODO: ใส่ Consumer ของ RabbitMQ ที่นี่
            // การสร้าง Consumer ใน RabbitMQ ดูได้ที่
            // configure.AddConsumer<SampleKafkaConsumer>();

            // TODO: มีการใช้ Request Client
            // การสร้าง Request Client ใน RabbitMQ ดูได้ที่
            // configure.AddRequestClient<DebtCancel>();

            return configure;
        }

        public static IEnumerable<KafkaConsumerSetting> ConfigKafkaConsumer(string projectName)
        {
            // TODO: ใส่ Consumer ของ Kafka ที่นี่
            // การสร้าง Consumer ใน Kafka ดูได้ที่
            // yield return new KafkaConsumerSetting<SampleKafkaConsumer,Ignore,Null>("SampleTopic", projectName, AutoOffsetReset.Earliest);

            // TODO: กรณีที่ใช้ Kafka Consumer เอาบรรทัดด้านล่างออก
            return null;
        }

        public static IEnumerable<KafkaProducerSetting> ConfigKafkaProducer()
        {
            // TODO: ใส่ Producer ของ Kafka ที่นี่
            // การสร้าง Producer ใน Kafka ดูได้ที่
            // yield return new KafkaProducerSetting<SampleMessage>("SampleTopic");

            // TODO: กรณีที่ใช้ Kafka Producer เอาบรรทัดด้านล่างออก
            return null;
        }
    }
}