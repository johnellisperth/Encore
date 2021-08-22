using Encore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows.Forms;

namespace Encore
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Application.Run(new FormMain());
            var builder = new HostBuilder()
               .ConfigureServices((hostContext, services) =>
               {
                   services.AddScoped<FormMain>();
                   services.AddScoped<BackupService>();

                   //Add Serilog
                   var serilogLogger = new LoggerConfiguration()
                           //.WriteTo.File("C:\\logs\\TheCodeBuzz.txt")
                           .CreateLogger();
                   services.AddLogging(x =>
                   {
                       x.SetMinimumLevel(LogLevel.Information);
                       x.AddSerilog(logger: serilogLogger, dispose: true);
                   });

               });

            var host = builder.Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                try
                {
                    var form1 = services.GetRequiredService<FormMain>();
                    Application.Run(form1);

                    Console.WriteLine("Success");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Occured");
                }
            }

        }
    }
}
