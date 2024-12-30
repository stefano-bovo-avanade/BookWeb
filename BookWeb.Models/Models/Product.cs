using System.ComponentModel.DataAnnotations;

namespace BookWeb.Models.Models
{
    public class Product
    {
        public const string F_PRODUCTS = "Products";
        public const string F_ID = "Id";
        public const string F_TITLE = "Title";
        public const string F_DESCRIPTION = "Description";
        public const string F_ISBN = "ISBN";
        public const string F_AUTHOR = "Author";
        public const string F_LISTPRICE = "ListPrice";
        public const string F_PRICE = "Price";
        public const string F_PRICE50 = "Price50";
        public const string F_PRICE100 = "Price100";


        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string ISBN { get; set; }

        public string Author { get; set; }

        [Required]
        [Range(1,1000)]
        [Display(Name = "List Price")]
        public double ListPrice { get; set; }
        
        [Required]
        [Range(1, 1000)]
        [Display(Name = "Price for 1-50")]
        public double Price { get; set; }

        [Required]
        [Range(1, 1000)]
        [Display(Name = "Price for 50+")]
        public double Price50 { get; set; }

        [Required]
        [Range(1, 1000)]
        [Display(Name = "Price for 100+")]
        public double Price100 { get; set; }

    }
}
