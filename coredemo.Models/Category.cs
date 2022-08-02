using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace coredemo.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name{ get; set; }
        [DisplayName("Display Order")]
        [Range(2,100,ErrorMessage ="Display Order should be in range between 2 and 100!")]
        public int Displayorderid { get; set; }
        public DateTime date { get; set; } = DateTime.Now;
    }
}
