using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// 1. Telemetri & Loggning (Application Insights)
builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Services.ConfigureFunctionsApplicationInsights();

// 2. Azure Clients (Blob & Queue)
builder.Services.AddAzureClients(clientBuilder =>
{
    var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

    // Arkitekturell notis: AzureWebJobsStorage används här eftersom det är Functions standardlagring.
    // I en produktionsmiljö bör man överväga att separera applikationsdata till en egen connection string 
    // (t.ex. "ImageStorageData") för att inte blanda affärsdata med Functions interna state.
    clientBuilder.AddBlobServiceClient(storageConnectionString);
    clientBuilder.AddQueueServiceClient(storageConnectionString);
});

// 3. Registrera egna Services och Repositories här nedanför
 //builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();

builder.Build().Run();