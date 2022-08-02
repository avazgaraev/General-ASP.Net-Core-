using coredemo.DataAccess.Repository.IRepository;
using coredemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coredemo.DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationDBContext _db;

        public OrderDetailRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }
  

        public void Update(OrderDetail obj)
        {
            _db.Update(obj);
        }
    }
}
