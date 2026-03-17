using Serilog.Formatting.Display;
using Serilog;
using Serilog.Sinks.WinForms.Base;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.Email;
using Serilog.Core;
using Microsoft.Extensions.Hosting;
using System.Net;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System;
using Serilog.Events;
using MailKit.Security; // 需要 MailKit 包

namespace deletedata
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
            
            Log.Logger = new LoggerConfiguration()
             .ReadFrom.Configuration(configuration)
            .MinimumLevel.Information()
            .WriteTo.Email(

               from: "4311465@qq.com",
               to: "4311465@qq.com",
               host:"smtp.qq.com",
               port:587,
              credentials: new System.Net.NetworkCredential(
                    4311465.ToString(),
                    "csaofqzkwfrsbjjj"
                ),
              subject: "日志邮件",
              body: "{Timestamp} [{Level}] {Message}{NewLine}{Exception}",
              formatProvider: null,
              restrictedToMinimumLevel: LogEventLevel.Error,
              levelSwitch: null
      
              
            )
            .WriteToSimpleAndRichTextBox(
             new MessageTemplateTextFormatter("{Timestamp} [{Level}] {Message} {Exception}\r\n"))
             .CreateLogger();

            Log.Fatal("Application started");
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());

        }
    }
}