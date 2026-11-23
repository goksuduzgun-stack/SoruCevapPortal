using Microsoft.EntityFrameworkCore;
using SoruCevapPortal.Data;
using SoruCevapPortal.Models;

namespace SoruCevapPortal.Repositories;

public class QuestionRepository
{
    private readonly ApplicationDbContext _context;

    public QuestionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<Question>> GetListAsync() =>
        _context.Questions
            .AsNoTracking()
            .Include(question => question.Category)
            .Include(question => question.Answers)
            .OrderByDescending(question => question.CreatedAt)
            .ToListAsync();

    public Task<Question?> GetByIdAsync(int id) =>
        _context.Questions
            .Include(question => question.Category)
            .Include(question => question.Answers)
            .FirstOrDefaultAsync(question => question.Id == id);

    public async Task AddAsync(Question question)
    {
        _context.Questions.Add(question);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Question model)
    {
        var question = await _context.Questions.FindAsync(model.Id);
        if (question == null)
            return false;

        question.Title = model.Title;
        question.Content = model.Content;
        question.CategoryId = model.CategoryId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
            return false;

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<List<Question>> SearchAsync(string? term, int? categoryId)
    {
        var query = _context.Questions
            .AsNoTracking()
            .Include(question => question.Category)
            .Include(question => question.Answers)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
        {
            var searchTerm = term.Trim();
            query = query.Where(question =>
                question.Title.Contains(searchTerm) ||
                question.Content.Contains(searchTerm));
        }

        if (categoryId.HasValue)
            query = query.Where(question => question.CategoryId == categoryId.Value);

        return query
            .OrderByDescending(question => question.CreatedAt)
            .ToListAsync();
    }
}
