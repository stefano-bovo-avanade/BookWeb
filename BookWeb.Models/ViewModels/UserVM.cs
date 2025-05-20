using BookWeb.Models.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookWeb.Models.ViewModels
{
    public class UserVM
    {
        public InputUser InputUser { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; }

        public IEnumerable<SelectListItem> Companies { get; set; }
    }
}
