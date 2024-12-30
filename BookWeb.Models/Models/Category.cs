using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookWeb.Models.Models
{
    public class Category
    {
        public const string F_NOME = "Name";
        public const string F_DISPLAYORDER = "DisplayOrder";
        public const string F_CATEGORY = "Categories";
        public const string F_ID = "Id";

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
