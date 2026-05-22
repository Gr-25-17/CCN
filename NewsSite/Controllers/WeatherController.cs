using Microsoft.AspNetCore.Mvc;
using NewsSite.Mapping;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;

namespace NewsSite.Controllers;

public class WeatherController(IWeatherService weatherService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Widget(string? city = null, bool detailed = false)
    {
        var weather = await weatherService.GetWeatherAsync(city);

        return detailed
            ? PartialView("~/Views/Shared/Components/WeatherCardVC/Detailed.cshtml", weather?.ToWeatherViewModel() ?? new WeatherViewModel())
            : PartialView("~/Views/Shared/Components/WeatherCardVC/Default.cshtml", weather?.ToViewModel() ?? new WeatherBasicVM());
    }

    [HttpGet]
    public async Task<IActionResult> Card(string? city = null)
    {
        var weather = await weatherService.GetWeatherAsync(city);
        return PartialView("~/Views/Shared/Partials/_WeatherCard.cshtml", weather?.ToWeatherViewModel() ?? new WeatherViewModel());
    }
}