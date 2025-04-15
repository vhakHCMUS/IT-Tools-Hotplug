public interface IToolRepository : IGenericRepository<Tool>
{
    Task<IEnumerable<Tool>> GetByKeywordAsync(string keyword);
    Task<Tool?> GetByNameAsync(string name);
    Task<IEnumerable<Tool>> GetAllAsync();
    Task<IEnumerable<Tool>> GetAllAvailableAsync();
    Task AddAsync(Tool tool);
}
