using Microsoft.AspNetCore.Mvc.Rendering;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;

namespace NewsSite.Mapping
{
    public static class CategoryExtensions
    {
        public static CategoryViewModel ToCategoryViewModel(this Category category)
        {
            return new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name
            };
        }
        public static SelectListItem ToCategorySelectListItem(this Category category)
        {
            return new SelectListItem
            {
                Value = category.Id.ToString(),
                Text = category.Name
            };
        }
    }
}