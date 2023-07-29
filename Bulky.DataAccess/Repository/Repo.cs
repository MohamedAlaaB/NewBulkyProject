using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class Repo<T> : IRepo<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> dbSet;
        public Repo( ApplicationDbContext context) 
        {
            _context = context;
            this.dbSet = _context.Set<T>();
            
        }
        public T Get(Expression<Func<T, bool>> filter ,bool tracking =false,string includeprops = null)
        {
            IQueryable<T> query;
            if (tracking == true )
            {

                query = dbSet;
            }
            else
            {
                query = dbSet.AsNoTracking();
               
            }
           
            query = query.Where(filter);
            if (includeprops != null)
            {
                foreach (var item in includeprops.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
          
            return query.FirstOrDefault();

        }

        public IEnumerable<T> GetAll( string? includeprops = null, Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (includeprops != null)
            {
                foreach (var item in includeprops.Split(new char[]{ ',' },StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
            return query.ToList();
        }

        public void Remove(T item)
        {
            dbSet.Remove(item);
        }

        public void RemoveRange(IEnumerable<T> items)
        {
           dbSet.RemoveRange(items);
        }

        public void Add(T item)
        {
            dbSet.Add(item);
        }
    }
}
