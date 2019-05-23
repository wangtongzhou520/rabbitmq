using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace OrderDal
{

    public interface IBaseDal<T> where T:class,new()
    {
        DbSqlSugarClient DbContext { get; }

        IBaseDal<T> UserDb(string dbName);
        IInsertable<T> AsInsertable(T t);
        IInsertable<T> AsInsertable(T[] t);
        IInsertable<T> AsInsertable(List<T> t);
        IUpdateable<T> AsUpdateable(T t);
        IUpdateable<T> AsUpdateable(T[] t);
        IUpdateable<T> AsUpdateable(List<T> t);
        IDeleteable<T> AsDeleteable();

        List<T> GetList();
        Task<List<T>> GetListAnsync();

        List<T> GetList(Expression<Func<T,bool>> whereExpression);
        Task<List<T>> GetListAnsync(Expression<Func<T, bool>> whereExpression);

        List<T> GetList(Expression<Func<T, bool>> whereExpression, Expression<Func<T, object>> orderExpression, OrderByType orderByType = OrderByType.Desc);
        Task<List<T>> GetListAnsync(Expression<Func<T, bool>> whereExpression, Expression<Func<T, object>> orderExpression, OrderByType orderByType = OrderByType.Desc);

        List<T> GetPageList(Expression<Func<T, bool>> whereExpression, PageModel page);
        Task<List<T>> GetPageListAsync(Expression<Func<T, bool>> whereExpression, PageModel page);

        List<T> GetPageList(Expression<Func<T, bool>> whereExpression, PageModel page, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc);
        Task<List<T>> GetPageListAsync(Expression<Func<T, bool>> whereExpression, PageModel page, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc);

        int Count(Expression<Func<T, bool>> whereExpression);
        Task<int> CountAsync(Expression<Func<T, bool>> whereExpression);
        T GetById(dynamic id);
        T GetSingle(Expression<Func<T, bool>> whereExpression);
        Task<T> GetSingleAsync(Expression<Func<T, bool>> whereExpression);
        T GetFirst(Expression<Func<T, bool>> whereExpression);
        Task<T> GetFirstAsync(Expression<Func<T, bool>> whereExpression);

        bool IsAny(Expression<Func<T, bool>> whereExpression);
        Task<bool> IsAnyAsync(Expression<Func<T, bool>> whereExpression);

        bool Insert(T t);
        Task<bool> InsertAsync(T t);
        bool InsertRange(List<T> t);
        Task<bool> InsertRangeAsync(List<T> t);
        bool InsertRange(T[] t);
        Task<bool> InsertRangeAsync(T[] t);
        int InsertReturnIdentity(T t);
        Task<long> InsertReturnIdentityAsync(T t);


        bool Delete(Expression<Func<T, bool>> whereExpression);
        Task<bool> DeleteAsync(Expression<Func<T, bool>> whereExpression);
        bool Delete(T t);
        Task<bool> DeleteAsync(T t);
        bool DeleteById(dynamic id);
        Task<bool> DeleteByIdAsync(dynamic id);
        bool DeleteByIds(dynamic[] ids);
        Task<bool> DeleteByIdsAsync(dynamic[] ids);


        bool Update(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression);
        Task<bool> UpdateAsync(Expression<Func<T, T>> columns, Expression<Func<T, bool>> whereExpression);
        bool Update(T t);
        Task<bool> UpdateAsync(T t);
        bool UpdateRange(T[] t);
        Task<bool> UpdateRangeAsync(T[] t);


        void BeginTran();
        void CommitTran();
        void RollbackTran();


    }
}
