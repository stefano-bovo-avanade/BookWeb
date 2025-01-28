using BookWeb.Models.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWeb.Models.ViewModels
{
    public class CategoryVM
    {
        public Product Product { get; set; }

        public IEnumerable<Category> CategoryList { get; set; }

        public IEnumerable<SelectListItem> CategorySelectList { get; set; }
    }
}
