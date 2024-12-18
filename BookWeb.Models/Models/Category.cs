using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookWeb.Models.Models
{
    public class Category
    {
        public static string F_ID = "Id";
        public static string F_NOME = "Name";
        public static string F_DISPLAYORDER = "DisplayOrder";
        public static string F_CATEGORY = "Categories";

        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        [DisplayName("Category Name")]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1,100)]
        public int DisplayOrder { get; set; }


    }
}
