using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookWeb.Models.Models
{
    public class Product : Entity
    {
        public static string F_TITLE = "Title";
        public static string F_DESCRIPTION = "Description";
        public static string F_ISBN = "ISBN";
        public static string F_AUTHOR = "Author";

        [Required]
        [Column("Title")]
        public string Title { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        [Column("ISBN")]
        public string ISBN { get; set; }

        [Column("Author")]
        public string Author { get; set; }

        [Required]
        [Range(1,1000)]
        [Display(Name = "List Price")]
        [Column("ListPrice")]
        public double ListPrice { get; set; }
        
        [Required]
        [Range(1, 1000)]
        [Display(Name = "Price for 1-50")]
        [Column("Price")]
        public double Price { get; set; }

        [Required]
        [Range(1, 1000)]
        [Display(Name = "Price for 50+")]
        [Column("Price50")]
        public double Price50 { get; set; }

        [Required]
        [Range(1, 1000)]
        [Display(Name = "Price for 100+")]
        [Column("Price100")]
        public double Price100 { get; set; }

    }
}
