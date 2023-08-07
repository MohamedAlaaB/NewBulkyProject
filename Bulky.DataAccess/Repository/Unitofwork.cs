using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class Unitofwork : IUnitofwork
    {
        private  ApplicationDbContext _context;
        public ICategory Category { get; private set; }

        public IProduct Product { get; private set; }

        public ICompany Company { get; private set; }

        public IShoppingCart ShoppingCart { get; private set; }
        public IAppUser AppUser { get; private set; }

        public IOrderHeader OrderHeader { get; private set; }
        public IOrderDetail OrderDetail { get; private set; }

        public IImage image { get; private set; }
        public Unitofwork(ApplicationDbContext context)
        {
            _context = context;
            Category = new CategoryRepo(_context);
            Product = new ProductRepo(_context);
            Company = new CompanyRepo(_context);
            AppUser = new AppUserRepo(_context);
            ShoppingCart = new ShoppingCartRepo(_context);
            OrderHeader = new OrderHeaderRepo(_context);
            OrderDetail = new OrderDetailRepo(_context);
            image = new ImgaeRepo(_context);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
