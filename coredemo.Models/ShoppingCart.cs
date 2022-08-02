using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coredemo.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product product { get; set; }
        public int Count{ get; set; }
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }
        [NotMapped]
        public double Price { get; set; }
    }
}
