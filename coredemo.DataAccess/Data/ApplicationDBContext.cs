using coredemo.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace coredemo.DataAccess
{
    public class ApplicationDBContext: IdentityDbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) :base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Various> Variouses{ get; set; }
        public DbSet<Product> Products{ get; set; }
        public DbSet<ApplicationUser> ApplicationUsers{ get; set; }
        public DbSet<Company> Companies{ get; set; }
        public DbSet<ShoppingCart> ShoppingCarts{ get; set; }
        public DbSet<OrderHeader> OrderHeaders{ get; set; }
        public DbSet<OrderDetail> OrderDetails{ get; set; }
    }
}
