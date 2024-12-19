using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookWeb.Models.Models
{
    public class Entity
    {
        public static string F_ID = "Id";
        [Key]
        [Column("Id")]
        public int Id { get; set; }
    }
}
