namespace TKPM_Project.Repositories
{
    public interface IUserLikedToolRepository
    {
        Task LikeToolAsync(string userId, int toolId);
        Task UnlikeToolAsync(string userId, int toolId);
        Task<bool> IsToolLikedAsync(string userId, int toolId);
        Task<IEnumerable<Tool>> GetLikedToolsAsync(string userId);
    }

}
