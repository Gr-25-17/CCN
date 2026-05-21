using Microsoft.AspNetCore.Mvc;
using NewsSite.Mapping;
using NewsSite.Services.Interfaces;
using NewsSite.Models.ViewModels

namespace NewsSite.ViewComponents;

public class WeatherCardVC(IWeatherService weatherService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(bool detailed = false, string? city = null)
    {
        var weather = await weatherService.GetWeatherAsync(city);

        return detailed
            ? View("Detailed", weather.ToWeatherViewModel() ?? new WeatherViewModel())
            : View("Default", weather.ToViewModel() ?? new WeatherBasicVM());
    }
}