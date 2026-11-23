using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SoruCevapPortal.Data;

namespace SoruCevapPortal.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _set;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public Task<List<T>> GetAllAsync() => _set.ToListAsync();
    public Task<T?> GetByIdAsync(int id) => _set.FindAsync(id).AsTask();
    public Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate) => _set.Where(predicate).ToListAsync();
    public async Task AddAsync(T entity) { await _set.AddAsync(entity); }
    public void Update(T entity) => _set.Update(entity);
    public void Remove(T entity) => _set.Remove(entity);
    public Task SaveAsync() => _context.SaveChangesAsync();
}
