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
}
