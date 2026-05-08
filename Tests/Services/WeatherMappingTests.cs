using FluentAssertions;
using NewsSite.Mapping;
using NewsSite.Models.APIs;
using NewsSite.Models.ViewModels;

namespace Tests.Services;

public class WeatherMappingTests
{
    [Theory]
    [InlineData(-5)]
    [InlineData(15)]
    public void ToViewModel_ShouldMapCorrectly(int temp)
    {
        var forecast = new WeatherForecast
        {
            TemperatureC = temp,
            Icon = new Icon { Url = "http://icon.com" }
        };

        // Anropar .ToViewModel() extension-metoden
        var result = forecast.ToViewModel();

        result.Should().BeOfType<WeatherBasicVM>();
        result.TemperatureC.Should().Be(temp);
        result.UrlIcon.Should().Be("http://icon.com");
    }
}