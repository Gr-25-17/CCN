using FluentAssertions;
using NewsSite.Mapping;
using NewsSite.Models.APIs;
using NewsSite.Models.ViewModels;
using Xunit;

namespace NewsSite.Tests.Services;

public class WeatherMappingTests
{
    [Theory]
    [InlineData(-5)]
    [InlineData(10)]
    [InlineData(25)]
    public void ToViewModel_ShouldMapTemperatureCorrectly(int temp)
    {
        var forecast = new WeatherForecast
        {
            City = "Stockholm",
            TemperatureC = temp,
            Icon = new Icon { Url = "http://test.com/img.png" }
        };

        // Använder din extension-metod från NewsSite.Mapping
        var result = forecast.ToViewModel();

        result.Should().BeOfType<WeatherBasicVM>();
        result.TemperatureC.Should().Be(temp);
        result.UrlIcon.Should().Be("http://test.com/img.png");
        result.TemperatureDisplay.Should().Be($"{temp} °C");
    }
}