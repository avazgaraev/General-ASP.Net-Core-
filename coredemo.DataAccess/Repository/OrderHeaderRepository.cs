using coredemo.DataAccess.Repository.IRepository;
using coredemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coredemo.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDBContext _db;

        public OrderHeaderRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }
  

        public void Update(OrderHeader obj)
        {
            _db.Update(obj);
        }


        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var value = _db.OrderHeaders.FirstOrDefault(x=>x.Id==id);
            if (value != null)
            {
                value.OrderStatus = orderStatus;
                if (value.PaymentStatus != null)
                {
                    value.PaymentStatus = paymentStatus;
                }
            }
        }
        
        public void UpdateStripeSettings(int id, string sessionId, string paymentIntentId)
        {
            var value = _db.OrderHeaders.FirstOrDefault(x=>x.Id==id);
            value.PaymentDate = DateTime.Now;

            value.SessionId = sessionId;
            value.PaymentIntentId = paymentIntentId;
        }
    }
}
