using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IRepo<T> where T : class
    {
        void Add(T item);
        T Get(Expression<Func<T,bool>> filter, bool tracking =false , string? includeprops = null );
        IEnumerable<T> GetAll( string? includeprops = null, Expression<Func<T, bool>>? filter=null);
        void Remove(T item);
        void RemoveRange(IEnumerable<T> items);
    }
}
