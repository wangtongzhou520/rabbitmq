using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace OrderDal
{
    public class BaseDal<T> : IBaseDal<T> where T : class, new()
    {
        private readonly IEnumerable<DbSqlSugarClient> clients;
        public BaseDal(IEnumerable<DbSqlSugarClient> clients)
        {
            this.clients = clients;
            DbContext = this.clients.FirstOrDefault(x => x.Default);
        }
        public DbSqlSugarClient DbContext { get; set; }

        public IDeleteable<T> AsDeleteable()
        {
            return DbContext.Deleteable<T>();
        }

        public IInsertable<T> AsInsertable(T t)
        {
            return DbContext.Insertable<T>(t);
        }

        public IInsertable<T> AsInsertable(T[] t)
        {
            return DbContext.Insertable<T>(t);
        }

        public IInsertable<T> AsInsertable(List<T> t)
        {
            return DbContext.Insertable<T>(t);
        }

        public IUpdateable<T> AsUpdateable(T t)
        {
            return DbContext.Updateable<T>(t);
        }

        public IUpdateable<T> AsUpdateable(T[] t)
        {
            return DbContext.Updateable<T>(t);
        }

        public IUpdateable<T> AsUpdateable(List<T> t)
        {
            return DbContext.Updateable(t);
        }

        public void BeginTran()
        {
            DbContext.Ado.BeginTran();
        }

        public void CommitTran()
        {
            DbContext.Ado.CommitTran();
        }

        public int Count(Expression<Func<T, bool>> whereExpression)
        {
            return DbContext.Queryable<T>().Count(whereExpression);
        }

        public Task<int> CountAsync(Expression<Func<T, bool>> whereExpression)
        {
            return DbContext.Queryable<T>().CountAsync(whereExpression);
        }

        public bool Delete(Expression<Func<T, bool>> whereExpression)
        {
            return DbContext.Deleteable<T>().Where(whereExpression).ExecuteCommand() > 0;
        }

        public bool Delete(T t)
        {
            return DbContext.Deleteable<T>().ExecuteCommand() > 0;
        }

        public async Task<bool> DeleteAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await DbContext.Deleteable<T>().Where(whereExpression).ExecuteCommandAsync() > 0;
        }

        public async Task<bool> DeleteAsync(T t)
        {
            return await DbContext.Deleteable(t).ExecuteCommandAsync() > 0;
        }

        public bool DeleteById(dynamic id)
        {
            return DbContext.Deleteable<T>().In(id).ExecuteCommand() > 0;
        }

        public async Task<bool> DeleteByIdAsync(dynamic id)
        {
            return await DbContext.Deleteable<T>().In(id).ExecuteCommandAsync() > 0;
        }

        public bool DeleteByIds(dynamic[] ids)
        {
            return DbContext.Deleteable<T>().In(ids).ExecuteCommand() > 0;
        }

        public async Task<bool> DeleteByIdsAsync(dynamic[] ids)
        {
            return await DbContext.Deleteable<T>().In(ids).ExecuteCommandAsync() > 0;
        }

        public T GetById(dynamic id)
        {
            return DbContext.Queryable<T>().InSingle(id);
        }

        public T GetFirst(Expression<Func<T, bool>> whereExpression)
        {
            return DbContext.Queryable<T>().First(whereExpression);
        }

        public async Task<T> GetFirstAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await DbContext.Queryable<T>().FirstAsync(whereExpression);
        }

        public List<T> GetList()
        {
            return DbContext.Queryable<T>().ToList();
        }

        public List<T> GetList(Expression<Func<T, bool>> whereExpression)
        {
            return DbContext.Queryable<T>().Where(whereExpression).ToList();
        }

        public List<T> GetList(Expression<Func<T, bool>> whereExpression, Expression<Func<T, object>> orderExpression, OrderByType orderByType = OrderByType.Desc)
        {
            return DbContext.Queryable<T>().Where(whereExpression).OrderByIF(orderExpression != null, orderExpression, orderByType).Where(whereExpression).ToList();
        }

        public async Task<List<T>> GetListAnsync()
        {
            return await DbContext.Queryable<T>().ToListAsync();
        }

        public async Task<List<T>> GetListAnsync(Expression<Func<T, bool>> whereExpression)
        {
            return await DbContext.Queryable<T>().Where(whereExpression).ToListAsync();
        }

        public async Task<List<T>> GetListAnsync(Expression<Func<T, bool>> whereExpression, Expression<Func<T, object>> orderExpression, OrderByType orderByType = OrderByType.Desc)
        {
            return await DbContext.Queryable<T>().Where(whereExpression).OrderByIF(orderExpression != null, orderExpression, orderByType).Where(whereExpression).ToListAsync();
        }

        public List<T> GetPageList(Expression<Func<T, bool>> whereExpression, PageModel page)
        {
            return DbContext.Queryable<T>().Where(whereExpression).ToPageList(page.PageIndex,page.PageSize);
        }

        public List<T> GetPageList(Expression<Func<T, bool>> whereExpression, PageModel page, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
        {
           return DbContext.Queryable<T>().Where(whereExpression).OrderByIF(orderByExpression != null, orderByExpression, orderByType).Where(whereExpression).ToPageList(page.PageIndex, page.PageSize);
        }

        public async Task<List<T>> GetPageListAsync(Expression<Func<T, bool>> whereExpression, PageModel page)
        {
            return await DbContext.Queryable<T>().Where(whereExpression).ToPageListAsync(page.PageIndex, page.PageSize);
        }

        public async Task<List<T>> GetPageListAsync(Expression<Func<T, bool>> whereExpression, PageModel page, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
        {
            return await DbContext.Queryable<T>().Where(whereExpression).OrderByIF(orderByExpression != null, orderByExpression, orderByType).Where(whereExpression).ToPageListAsync(page.PageIndex, page.PageSize);
        }

        public T GetSingle(Expression<Func<T, bool>> whereExpression)
        {
            return DbContext.Queryable<T>().Single(whereExpression);
        }

        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await DbContext.Queryable<T>().SingleAsync(whereExpression);
        }

        public bool Insert(T t)
        {
            return DbContext.Insertable(t).ExecuteCommand() > 0;
        }

        public async Task<bool> InsertAsync(T t)
        {
            return await DbContext.Insertable(t).ExecuteCommandAsync() > 0;
        }

        public bool InsertRange(List<T> t)
        {
            return DbContext.Insertable(t).ExecuteCommand() > 0;
        }

        public bool InsertRange(T[] t)
        {
            return DbContext.Insertable(t).ExecuteCommand() > 0;
        }

        public async Task<bool> InsertRangeAsync(List<T> t)
        {
            return await DbContext.Insertable(t).ExecuteCommandAsync() > 0;
        }

        public async Task<bool> InsertRangeAsync(T[] t)
        {
            return await DbContext.Insertable(t).ExecuteCommandAsync() > 0;
        }

        public int InsertReturnIdentity(T t)
        {
            return DbContext.Insertable(t).ExecuteReturnIdentity();
        }

        public async Task<long> InsertReturnIdentityAsync(T t)
        {
            return await DbContext.Insertable(t).ExecuteReturnBigIdentityAsync();
        }

        public bool IsAny(Expression<Func<T, bool>> whereExpression)
        {
            return DbContext.Queryable<T>().Any(whereExpression);
        }

        public async Task<bool> IsAnyAsync(Expression<Func<T, bool>> whereExpression)
        {
            return await DbContext.Queryable<T>().AnyAsync(whereExpression);
        }

        public void RollbackTran()
        {
            DbContext.Ado.RollbackTran();
        }

        public bool Update(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression)
        {
            return DbContext.Updateable<T>().UpdateColumns(columns).Where(whereExpression).ExecuteCommand() > 0;
        }

        public bool Update(T t)
        {
            return DbContext.Updateable(t).ExecuteCommand() > 0;
        }

        public async Task<bool> UpdateAsync(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression)
        {
            return await DbContext.Updateable<T>().UpdateColumns(columns).Where(whereExpression).ExecuteCommandAsync() > 0;
        }

        public async Task<bool> UpdateAsync(T t)
        {
            return await DbContext.Updateable(t).ExecuteCommandAsync() > 0;
        }

        public bool UpdateRange(T[] t)
        {
            return DbContext.Updateable(t).ExecuteCommand() > 0;
        }

        public async Task<bool> UpdateRangeAsync(T[] t)
        {
            return await DbContext.Updateable(t).ExecuteCommandAsync() > 0;
        }

        public IBaseDal<T> UserDb(string dbName)
        {
            DbContext = this.clients.FirstOrDefault(it => it.DbName == dbName);
            return this;
        }
    }
}
