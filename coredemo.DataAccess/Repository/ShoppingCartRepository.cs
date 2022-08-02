using coredemo.DataAccess.Repository.IRepository;
using coredemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coredemo.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private ApplicationDBContext _db;

        public ShoppingCartRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }

        public int DecrementCount(ShoppingCart shoppingCart, int count)
        {
            shoppingCart.Count -= count;
            return shoppingCart.Count;
        }

        public int IncrementCount(ShoppingCart shoppingCart, int count)
        {
            shoppingCart.Count += count;
            return shoppingCart.Count;
        }
    }
}
