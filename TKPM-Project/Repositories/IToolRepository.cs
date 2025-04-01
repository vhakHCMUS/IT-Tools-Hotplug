public interface IToolRepository : IGenericRepository<Tool>
{
    Task<IEnumerable<Tool>> GetByKeywordAsync(string keyword);
}
