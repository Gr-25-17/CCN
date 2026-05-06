using Azure.Data.Tables;
using NewsSite.Models.APIs;
using NewsSite.Services.Interfaces;
namespace NewsSite.Services.Implementations
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherForecast> GetWeatherAsync()
        {
            try
            {
                var connectionString = "DefaultEndpointsProtocol=https;AccountName=ccnstorage;AccountKey=BFpTXfEqkCmNdZX9W3KeUxnyv44VtjrJXe4ZoxTCT+k0p+f7qpSriJ+xPn78Sxcgo885k18v7vUb+AStUJbl0Q==;EndpointSuffix=core.windows.net";

                var tableClient = new TableClient(connectionString, "WeatherData");



                var query = tableClient.QueryAsync<TableEntity>(
                    filter: "PartitionKey eq 'Weather'",
                    maxPerPage: 1
                );

                TableEntity? latestEntity = null;
                await foreach (var entity in query)
                {
                    if (latestEntity == null || string.Compare(entity.RowKey, latestEntity.RowKey) > 0)
                    {
                        latestEntity = entity;
                    }
                }

                if (latestEntity != null)
                {
                    return new WeatherForecast
                    {
                        City = latestEntity.GetString("City"),
                        TemperatureC = (int)latestEntity.GetDouble("TemperatureC").GetValueOrDefault(),
                        Humidity = (int)latestEntity.GetDouble("Humidity").GetValueOrDefault(),
                        WindSpeed = (int)latestEntity.GetDouble("WindSpeed").GetValueOrDefault(),
                        Date = DateTime.Parse(latestEntity.GetString("DateString") ?? DateTime.UtcNow.ToString()),
                        Icon = new Icon
                        {
                            Url = latestEntity.GetString("IconUrl") ?? string.Empty,
                            Code = latestEntity.GetString("IconCode") ?? string.Empty
                        }
                    };
                }

                return new WeatherForecast();
            }
            catch
            {
                return new WeatherForecast();
            }
        }
    }
}