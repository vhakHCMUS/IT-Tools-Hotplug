using TKPM_Project.Models;

namespace TKPM_Project.Repositories
{
    public interface IUserPremiumRepository
    {
        Task<UserPremium?> GetByUserIdAsync(string userId);
        Task AddAsync(UserPremium userPremium);
        Task<bool> IsPremiumActiveAsync(string userId);
        Task RemoveExpiredAsync();
        Task<List<UserPremium>> GetAllExpiredAsync();
        Task<List<UserPremium>> GetAllActiveAsync();


    }

}
