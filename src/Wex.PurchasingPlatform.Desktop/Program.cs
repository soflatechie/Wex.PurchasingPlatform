using Microsoft.Extensions.Configuration;
using Wex.PurchasingPlatform.Desktop.ApiClient;

namespace Wex.PurchasingPlatform.Desktop;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(configuration["Api:BaseUrl"]!)
        };

        var apiClient = new PurchasingApiClient(httpClient);

        Application.Run(new MainForm(apiClient));
    }
}