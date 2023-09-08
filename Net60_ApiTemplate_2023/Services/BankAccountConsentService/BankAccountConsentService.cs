using Microsoft.Extensions.Options;
using Serilog;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Text.Json;
using TTB.BankAccountConsent.Data;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using TTB.BankAccountConsent.Configurations;
using MassTransit.Configuration;
using TTB.BankAccountConsent.Services.Base;
using Renci.SshNet;
using System.Globalization;
using System.Text;
using TTB.BankAccountConsent.Models;
using Confluent.Kafka;
using MassTransit;
using Microsoft.VisualBasic.FileIO;
using static MassTransit.Util.ChartTable;
using MassTransit.Internals.GraphValidation;
using System.Reflection.Emit;
using EFCore.BulkExtensions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TTB.BankAccountConsent.Services.Crypto;

namespace TTB.BankAccountConsent.Services.BankAccountConsentService
{
    public class BankAccountConsentService : IBankAccountConsentService
    {

        private readonly AppDBContext _context;
        private readonly LocalPath _optLocal;
        private readonly Company _optCompany;
        private readonly LastModifiedConfig _optMod;
        private readonly IOptions<sFtpPath> _optsFtp;
        private readonly IWebHostEnvironment _environment;
        private readonly ICryptoServices _cryptoServices;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        public BankAccountConsentService(AppDBContext context, IOptions<LocalPath> optLocal, IOptions<Company> optCompany, IWebHostEnvironment environment, IOptions<sFtpPath> optsFtp, IOptions<LastModifiedConfig> optMod, ICryptoServices cryptoServices)
        {
            _context = context;
            _optLocal = optLocal.Value;
            _optMod = optMod.Value;
            _optCompany = optCompany.Value;
            _optsFtp = optsFtp;
            _environment = environment;
            _cryptoServices = cryptoServices;
            jsonSerializerOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };
        }

        public async Task DownloadFile()
        {
            try
            {
                //Crearte an instance of the ConfiigSfptBase class.
                //Crearte an instance of the ConfigDetail class.
                var configSftp = new ConfigSftpBase(_optsFtp);
                var configSftpDetail = new ConfigDetail();

                //Check Env and assign values to instance
                Log.Information("[TTBDownloadFile] - Environment={env}", _environment.EnvironmentName);

                //Is Development
                if (_environment.IsDevelopment())
                {
                    configSftpDetail = configSftp.ConfigSftpByCompany(companyId: "Development");
                    await DownloadAndInsertToDatabase(configSftpDetail);
                }
                else
                {
                    //List Company.
                    var companies = _optCompany.CompId;

                    configSftpDetail = configSftp.ConfigSftpByCompany(companyId: companies);
                    await DownloadAndInsertToDatabase(configSftpDetail);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[TTBDownloadFile] - DownloadFile Error");
            }
        }

        private async Task DownloadAndInsertToDatabase(ConfigDetail config)
        {
            try
            {
                //Preparations
                //Declare a variable and assigning the value
                var host = config.Server;
                var port = config.Port;
                var username = config.Username;
                var password = config.Password;

                //Connect to Sftp Server
                Log.Information("[TTBDownloadFile] - Connect to Sftp Server={host}:{port},Username={username}", host, port, username);
                SftpClient sftp = new SftpClient(host: host, port: int.Parse(port), username: username, password: password);
                sftp.Connect();

                if (sftp.IsConnected)
                {
                    //Assign calue to cariable
                    var remoteDirectory = config.Output;

                    //Get location path
                    var pathFile = _optLocal.Output;
                    var localPath = $"{pathFile}/{DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}";

                    //Check Directory
                    if (!Directory.Exists(localPath))
                        Directory.CreateDirectory(localPath);

                    // list file in folder Directory remote where last 3 is txt.
                    // and check equals last modified date
                    var lastmodified = DateTime.Now.AddDays(_optMod.IntDays);
                    var files = sftp.ListDirectory(remoteDirectory).Where(x => x.FullName.Substring(x.FullName.Length - 3, 3) == "pgp");
                    Log.Information("[TTBDownloadFile] - list files out [File Name]:{@file}", files.Select(_ => _.Name));

                    foreach (var file in files)
                    {
                        // Check duplicate file within a day
                        Log.Information("[TTBDownloadFile] - Check duplicate file within a day using [LINQ]: Check duplicate file within a day, [Param]: File={File}", file);
                        var validate = _context.BatchSources.Any(x => x.FileName.ToUpper() == file.Name.ToUpper() && x.IsActive == true);

                        if (validate)
                        {
                            Log.Information("[TTBDownloadFile] - Duplicate File, message=inValid:duplicate ReceiveFile {fileName}", file.Name);
                            continue;
                        }

                        //Declare source file path.
                        //Declare destination file path.
                        var sourceFilePath = Path.Combine(remoteDirectory, file.Name);
                        var destFilePath = Path.Combine(localPath, file.Name);

                        //Download and Create file output to local directory.
                        using (Stream output = File.Create(destFilePath))
                        {
                            Log.Information("[TTBDownloadFile] - Copying file from server to local={path}", destFilePath);
                            sftp.DownloadFile(sourceFilePath, output, (downloaded) =>
                            {
                                if (output.Length == (long)downloaded)
                                {
                                    Log.Information("[TTBDownloadFile] - Download file is success");

                                }
                                else
                                {
                                    Log.Information("[TTBDownloadFile] - Download file fail or number byte of file is not equal (file = {output}, download file lenth = {downloaded})", output.Length, (long)downloaded);
                                }
                            });

                            Log.Information("[TTBDownloadFile] - Delete file on the remote server in SFTP={fileName}", file.Name);
                            sftp.Delete(sourceFilePath);
                        };

                        //Decrypt file
                        FileInfo downloadedFile = new FileInfo(destFilePath);
                        var decryptFile = await _cryptoServices.DecryptFile(downloadedFile, localPath);
                        Log.Information("[TTBDownloadFile] - DecryptFile {@decryptFile}", decryptFile);

                        //Create text file ANSI
                        var windows874 = Encoding.Default;

                        //Read all lines file.
                        Log.Information("[TTBDownloadFile] - Reading file on the local..");
                        string[] allLines = File.ReadAllLines(decryptFile.FullName, windows874);

                        var running = 0;

                        Log.Information("[TTBDownloadFile] - Add Data List TTBBatchHeader");
                        List<TTBBatchHeader> ttbBatchHeader = new List<TTBBatchHeader>();
                        List<TTBBatchDetail> ttbBatchDetail = new List<TTBBatchDetail>();
                        List<TTBBatchTrailer> ttbBatchTrailer = new List<TTBBatchTrailer>();
                        Guid TTBHeaderId = Guid.NewGuid();
                        foreach (var line in allLines)
                        {
                            if (line.StartsWith("H"))
                            {
                                ttbBatchHeader.Add(new TTBBatchHeader
                                {
                                    TTBBatchHeaderId = TTBHeaderId,
                                    RecordType = line.Substring(0, 1).Trim(),
                                    SequenceNo = line.Substring(1, 6).Trim(),
                                    BankCode = line.Substring(7, 3).Trim(),
                                    CompId = line.Substring(10, 4).Trim(),
                                    CompanyName = line.Substring(14, 40).Trim(),
                                    PostDateTime = DateTime.ParseExact(line.Substring(54, 15), "ddMMyyyy_HHmmss", CultureInfo.InvariantCulture),
                                    Spare = line.Substring(69, 131).Trim(),
                                    PlainText = line,
                                    IsActive = true,
                                    CreatedDate = DateTime.Now,
                                    CreatedByUserId = 1,
                                    UpdatedDate = DateTime.Now,
                                    UpdatedByUserId = 1
                                });
                            }
                            else if (line.StartsWith("D"))
                            {
                                ttbBatchDetail.Add(new TTBBatchDetail
                                {
                                    TTBBatchDetailId = Guid.NewGuid(),
                                    TTBBatchHeaderId = TTBHeaderId,
                                    Seq = ++running,
                                    RecodeType = line.Substring(0, 1).Trim(),
                                    SequenceNo = line.Substring(1, 6).Trim(),
                                    BankCode = line.Substring(7, 3).Trim(),
                                    Action = line.Substring(10, 1).Trim(),
                                    ApplyDate = DateTime.ParseExact(line.Substring(11, 15), "ddMMyyyy_HHmmss", CultureInfo.InvariantCulture),
                                    Ref1 = line.Substring(26, 20).Trim(),
                                    Ref2 = line.Substring(46, 20).Trim(),
                                    AccountNo = line.Substring(66, 16).Trim(),
                                    AccountName = line.Substring(82, 50).Trim(),
                                    ChannelApply = line.Substring(132, 3).Trim(),
                                    Status = line.Substring(135, 1).Trim(),
                                    Spare = line.Substring(136, 64).Trim(),
                                    PlainText = line,
                                    IsActive = true,
                                    CreatedDate = DateTime.Now,
                                    CreatedByUserId = 1,
                                    UpdatedDate = DateTime.Now,
                                    UpdatedByUserId = 1
                                });
                            }else if (line.StartsWith("T"))
                            {
                                ttbBatchTrailer.Add(new TTBBatchTrailer
                                {
                                    TTBBatchTrailerId = Guid.NewGuid(),
                                    TTBBatchHeaderId = TTBHeaderId,
                                    RecodeType = line.Substring(0, 1).Trim(),
                                    LastSequenceNo = line.Substring(1, 6).Trim(),
                                    BankCode = line.Substring(7, 3).Trim(),
                                    TotalAddApplyRecords = Convert.ToInt32(line.Substring(10, 7).Trim()),
                                    TotalCancelRecords = Convert.ToInt32(line.Substring(17, 7).Trim()),
                                    Spare = line.Substring(24, 176).Trim(),
                                    PlainText = line,
                                    IsActive = true,
                                    CreatedDate = DateTime.Now,
                                    CreatedByUserId = 1,
                                    UpdatedDate = DateTime.Now,
                                    UpdatedByUserId = 1
                                });
                            }
                        }
                        using (var transectionDetail = _context.Database.BeginTransaction())
                        {
                            //Bulk Insert.
                            Log.Information("[TTBDownloadFile] - BulkInsert data into the table TTBBatchHeader");
                            await _context.BulkInsertAsync(ttbBatchHeader);
                            Log.Information("[TTBDownloadFile] - BulkInsert data into the table TTBBatchDetail");
                            await _context.BulkInsertAsync(ttbBatchDetail);
                            Log.Information("[TTBDownloadFile] - BulkInsert data into the table TTBBatchTrailer");
                            await _context.BulkInsertAsync(ttbBatchTrailer);
                            await transectionDetail.CommitAsync();
                        }

                        //Insert data into database using stored procedure
                        Log.Information("[TTBDownloadFile] - Insert data into database using [Stored Procedure]: usp_KTBSettlement_InsertAsync, [Param]: GroupId={groupId}, Path={destFilePath}, FileName={filename}", TTBHeaderId, destFilePath, file.Name);
                        var result = await _context.Procedures.usp_TTBBatchToBankAccountConsent_InsertAsync(TTBHeaderId, file.Name, destFilePath);
                        Log.Information("[TTBDownloadFile] - [Result]: {result}", JsonSerializer.Serialize(result, jsonSerializerOptions));

                    }
                    //Disconnect.
                    sftp.Disconnect();
                }
                else
                {
                    Log.Error("[TTBDownloadFile] - Connected sftp is fail");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[TTBDownloadFile] - DownloadFile Error");
            }
        }

    }

}
