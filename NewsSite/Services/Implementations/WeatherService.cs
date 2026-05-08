using Azure.Data.Tables;
using NewsSite.Models.APIs;
using NewsSite.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace NewsSite.Services.Implementations;

public class WeatherService(IHttpClientFactory httpClientFactory, IConfiguration config) : IWeatherService
{
    public async Task<WeatherForecast> GetWeatherAsync()
    {
        try
        {
            var connectionString = config.GetConnectionString("AzureWebJobsStorage");
            var tableClient = new TableClient(connectionString, "WeatherData");

            // Med inverterad RowKey ligger den senaste väderrapporten ALLTID först.
            var latestEntity = await tableClient.QueryAsync<TableEntity>(
                filter: "PartitionKey eq 'Weather'",
                maxPerPage: 1)
                .FirstOrDefaultAsync();

            if (latestEntity is null) return null;

            return new WeatherForecast
            {
                City = latestEntity.GetString("City") ?? "Okänd",
                TemperatureC = latestEntity.GetDouble("TemperatureC") ?? 0,
                Humidity = latestEntity.GetDouble("Humidity") ?? 0,
                WindSpeed = latestEntity.GetDouble("WindSpeed") ?? 0,
                Date = DateTime.TryParse(latestEntity.GetString("DateString"), out var d) ? d : DateTime.UtcNow,
                Icon = new Icon
                {
                    Url = latestEntity.GetString("IconUrl") ?? string.Empty,
                    Code = latestEntity.GetString("IconCode") ?? string.Empty
                }
            };
        }
        catch (Exception)
        {
            return null;
        }
    }
}