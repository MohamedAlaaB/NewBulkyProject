using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.EntityFrameworkCore;
using System;
using Bulky.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class CategoryRepo : Repo<Category>, ICategory
    {
        private readonly ApplicationDbContext _context;
       
        public CategoryRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
            
            
        }

        public void Update(Category category)
        {
            _context.Categories.Update(category);
        }
    }
}
