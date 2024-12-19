using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookWeb.Models.Models
{
    public class Category : Entity
    {
        public static string F_NOME = "Name";
        public static string F_DISPLAYORDER = "DisplayOrder";
        public static string F_CATEGORY = "Categories";

        [Required]
        [MaxLength(30)]
        [DisplayName("Category Name")]
        [Column("Name")]
        public string Name { get; set; }

        [DisplayName("Display Order")]
        [Range(1,100)]
        [Column("DisplayOrder")]
        public int DisplayOrder { get; set; }


    }
}
