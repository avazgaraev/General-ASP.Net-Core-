using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coredemo.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IVariousRepository Various { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IOrderDetailRepository OrderDetail { get; }
        IOrderHeaderRepository OrderHeader { get; }
        void Save();
    }
}
