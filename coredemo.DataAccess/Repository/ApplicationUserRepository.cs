using coredemo.DataAccess.Repository.IRepository;
using coredemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coredemo.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDBContext _db;

        public ApplicationUserRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }
  

      
    }
}
