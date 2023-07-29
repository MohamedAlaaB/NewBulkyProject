using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IUnitofwork
    {
        ICategory Category { get; }
        IProduct Product { get; }
        ICompany Company { get; }
        IShoppingCart ShoppingCart { get; }
        IAppUser AppUser { get; }
        IOrderDetail OrderDetail { get; }
        IOrderHeader OrderHeader { get; }
        void Save();
    }
   
}
