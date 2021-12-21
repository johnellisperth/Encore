using Encore.Helpers;
using Encore.Services;
using Encore.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Storage;

namespace Encore;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var builder = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                 
                //Add Serilog
                var serilogLogger = new LoggerConfiguration()
                        //.WriteTo.File("C:\\logs\\TheCodeBuzz.txt")
                        .CreateLogger();
                services.AddLogging(x =>
                {
                    x.SetMinimumLevel(LogLevel.Information);
                    x.AddSerilog(logger: serilogLogger, dispose: true);
                });

                services.AddScoped<IProgress<int>, Progress<int>>();
                services.AddScoped<ProgressManager>();
                services.AddScoped<SourceDestValidator>();
                services.AddScoped<SafeFileSystemHelper>();
                services.AddScoped<AppSettings>();
                services.AddScoped<SourceDestComparison>();
                services.AddScoped<BackupService>();
                services.AddScoped<FormMain>();

            });

        var host = builder.Build();

        using (var serviceScope = host.Services.CreateScope())
        {
            var services = serviceScope.ServiceProvider;
            try
            {
                var form_main = services.GetRequiredService<FormMain>();
                Application.Run(form_main);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Occured {ex.Message}");
            }
        }
    }
}

