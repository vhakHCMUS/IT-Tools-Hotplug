using Microsoft.EntityFrameworkCore;

public class ToolRepository : GenericRepository<Tool>, IToolRepository
{
    public ToolRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Tool>> GetByKeywordAsync(string keyword)
    {
        return await _context.Tools
            .Where(t => t.Name.Contains(keyword) || t.Description.Contains(keyword))
            .ToListAsync();
    }

    public async Task<IEnumerable<Tool>> GetAllAsync()
    {
        return await _context.Tools.ToListAsync();
    }

    public async Task<IEnumerable<Tool>> GetAllAvailableAsync()
    {
        return await _context.Tools.Where(t => t.IsAvailable).ToListAsync();
    }

    public async Task<Tool?> GetByNameAsync(string name)
    {
        return await _context.Tools.FirstOrDefaultAsync(t => t.Name == name);
    }
}
