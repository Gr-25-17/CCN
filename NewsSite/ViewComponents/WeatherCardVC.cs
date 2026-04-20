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

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var weather = await _weatherService.GetWeatherAsync();

                var vm = weather?.ToViewModel();
                return View(vm);
            }
            catch (Exception ex)
            {
                return View(null);
            }
        }
    }
    
}
