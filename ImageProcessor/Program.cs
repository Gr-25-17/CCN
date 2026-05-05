using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Registrera BlobServiceClient så att ImageWorker kan använda den[cite: 1, 13]
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration["AzureWebJobsStorage"];

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Anslutningssträngen 'AzureWebJobsStorage' saknas.");
    }

    return new BlobServiceClient(connectionString);
});

// builder.Services
//    .AddApplicationInsightsTelemetryWorkerService()
//    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();