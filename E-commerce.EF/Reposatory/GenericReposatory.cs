using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using E_commerce.Core.IReposatory;
using E_commerce.EF.Data;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.EF.Reposatory
{
    public class GenericReposatory<T> : IGenericReposatory<T> where T : class
    {
        private readonly ApplicationDbContext context;
        public GenericReposatory(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IQueryable<T> GetQueryable(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> quary = context.Set<T>();
            if (quary != null)
            {
                foreach (var item in includes)
                    quary = quary.Include(item);
            }
            return quary;
        }
        public async Task<List<T>> GetAll(params Expression<Func<T, object>>[] includes)
        {
            var quary = GetQueryable(includes);
            return await quary.ToListAsync();
        }

        public async Task<List<T>> GetAllwithsearch(Expression<Func<T, bool>> filter=null, params Expression<Func<T, object>>[] includes)
        {
            var quary = GetQueryable().Where(filter);
            return await quary.ToListAsync();
        }

        public async Task<T> GetById(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            var quary= GetQueryable(includes);
            return await quary.FirstOrDefaultAsync(predicate);

        }
        public async Task<List<T>> FindAll(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            var quary = GetQueryable(includes);
            return await quary.Where(predicate).ToListAsync();
        }
 

        public async Task Create(T item)
        {
            await context.Set<T>().AddAsync(item);
        }

        public void update(T item)
        {
             context.Set<T>().Update(item);
        }

        public void delete(T item)
        {
            context.Set<T>().Remove(item);
        }

        public void Save()
        {
             context.SaveChanges();
        }
    }
}
