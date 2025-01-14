using Core.Entities;

namespace Core.Interfaces;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task AddAsync(T entity);
    void Delete(T entity);
    Task DeleteByIdAsync(Guid id);
    Task<T?> FindByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
}