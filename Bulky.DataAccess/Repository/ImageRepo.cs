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
    public class ImgaeRepo : Repo<Images>, IImage
    {
        private readonly ApplicationDbContext _context;
       
        public ImgaeRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
            
            
        }

        public void Update(Images images)
        {
            _context.Images.Update(images);
        }
    }
}
