using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coredemo.Models
{
    public class Various
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Type")]
        public string Name { get; set; }
    }
}
