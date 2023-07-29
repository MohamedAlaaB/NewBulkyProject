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
    public class ProductRepo : Repo<Product>, IProduct
    {
        private ApplicationDbContext _context;
        public ProductRepo(ApplicationDbContext context):base(context) 
        {
                _context = context;
        }

        public void Update(Product product)
        {
           var obj= _context.Products.FirstOrDefault(p=>p.Id == product.Id);
            if (obj != null)
            {               
                obj.Title = product.Title;
                obj.Description = product.Description;
                obj.ISBN = product.ISBN;
                obj.Author = product.Author;
                obj.ListPrice = product.ListPrice;
                obj.Price = product.Price;
                obj.Price50 = product.Price50;
                obj.Price100 = product.Price100;
                obj.CategoryId = product.CategoryId;
                if (product.ImageUrl != null)
                {
                    obj.ImageUrl = product.ImageUrl;
                }
            }
        }

       
    }
}
