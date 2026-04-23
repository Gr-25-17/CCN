using Microsoft.AspNetCore.Mvc;
using NewsSite.Mapping;
using NewsSite.Services.Implementations;

namespace NewsSite.ViewComponents
{
    public class WeatherCardVC : ViewComponent
    {
    
            private readonly WeatherService _weatherService;

            public WeatherCardVC(WeatherService weatherService)
            {
                _weatherService = weatherService;
            }

        public async Task<IViewComponentResult> InvokeAsync(bool detailed = false)
        {
            try
            {
                var weather = await _weatherService.GetWeatherAsync();

                if (detailed)
                {
                    var vm = weather?.ToWeatherViewModel();
                    return View("Detailed", vm);
                }
                else
                {
                    var vm = weather?.ToViewModel();
                    return View("Default", vm);
                }
            }
            catch (Exception)
            {
                return View("Default");
            }
        }
    }
    
}
