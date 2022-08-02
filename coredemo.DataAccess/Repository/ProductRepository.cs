using coredemo.DataAccess.Repository.IRepository;
using coredemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coredemo.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDBContext _db;

        public ProductRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }
  

        public void Update(Product obj)
        {
            var value = _db.Products.FirstOrDefault(x=>x.Productid == obj.Productid);
            if (value != null)
            {
                value.ISBN = obj.ISBN;
                value.Author = obj.Author;
                value.Description = obj.Description;
                value.Price = obj.Price;
                value.ListPrice = obj.ListPrice;
                value.CategoryId = obj.CategoryId;
                value.Price50 = obj.Price50;
                value.Price100 = obj.Price100;
                value.Title = obj.Title;
                value.VariousId = obj.VariousId;
                if (value.ImgURL != null)
                {
                    value.ImgURL = obj.ImgURL;
                }
            }
        }
    }
}
