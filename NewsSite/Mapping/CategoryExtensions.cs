using Microsoft.AspNetCore.Mvc.Rendering;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;

namespace NewsSite.Mapping
{
    public static class CategoryExtensions
    {
        public static CategoryViewModel ToCategoryViewModel(this Category category)
            => new()
            {
                Id = category.Id,
                Name = category.Name
            };

        public static SelectListItem ToCategorySelectListItem(this Category category)
            => new()
            {
                Value = category.Id.ToString(),
                Text = category.Name
            };
    }
}
