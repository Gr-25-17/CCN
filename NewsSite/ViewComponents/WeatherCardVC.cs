using Microsoft.AspNetCore.Mvc;
using NewsSite.Mapping;
using NewsSite.Services.Interfaces;

namespace NewsSite.ViewComponents;

public class WeatherCardVC(IWeatherService weatherService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(bool detailed = false)
    {
        var weather = await weatherService.GetWeatherAsync();

        return detailed
            ? View("Detailed", weather.ToWeatherViewModel())
            : View("Default", weather.ToViewModel());
    }
}