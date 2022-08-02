using coredemo.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace coredemo.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDBContext _db;

        internal DbSet<T> _dbset;

        public Repository(ApplicationDBContext db)
        {
            _db = db;
            //_db.ShoppingCarts.Include(u=>u.pr)
            this._dbset = _db.Set<T>();
        }

        public void Add(T entity)
        {
            _dbset.Add(entity);
        }

        public void Delete(T entity)
        {
            _dbset.Remove(entity);
        }

        public void DeleteMore(IEnumerable<T> entity)
        {
            _dbset.RemoveRange(entity);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null, string? includeProperties = null)
        {
            IQueryable<T> query = _dbset;
            if (filter!=null)
            {
                query = query.Where(filter);
            }
            if (includeProperties!=null)
            {
                foreach (var prop in includeProperties.Trim().Split(new char [] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    query =  query.Include(prop);
                }
            }
            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true)
        {
            IQueryable<T> query;
            if (tracked)
            {
                query = _dbset;
            }
            else
            {
                query= _dbset.AsNoTracking();
            }
            query = query.Where(filter);
            if (includeProperties != null)
            {
                foreach (var prop in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(prop);
                }
            }
            return query.FirstOrDefault();
        }
    }
}
