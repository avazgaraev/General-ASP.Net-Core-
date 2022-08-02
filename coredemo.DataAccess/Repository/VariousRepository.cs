using coredemo.DataAccess.Repository.IRepository;
using coredemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coredemo.DataAccess.Repository
{
    public class VariousRepository : Repository<Various>, IVariousRepository
    {
        private ApplicationDBContext _db;

        public VariousRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }
  

        public void Update(Various obj)
        {
            _db.Update(obj);
        }
    }
}
