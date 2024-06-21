using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    private readonly QuizDbContext _context;
    private readonly DbSet<T?> _dbSet;

    public BaseRepository(QuizDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T?>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<T?>> GetByConditionAsync(Expression<Func<T?, bool>> expression)
    {
        return await _dbSet.Where(expression).ToListAsync();
    }

    /// <inheritdoc />
    public async Task AddAsync(T? entity)
    {
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(T? entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(T? entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }
}