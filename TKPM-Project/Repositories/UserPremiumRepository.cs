using System;
using TKPM_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace TKPM_Project.Repositories
{
    public class UserPremiumRepository : IUserPremiumRepository
    {
        private readonly ApplicationDbContext _context;

        public UserPremiumRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserPremium?> GetByUserIdAsync(string userId)
        {
            return await _context.UserPremiums
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task AddAsync(UserPremium userPremium)
        {
            _context.UserPremiums.Add(userPremium);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsPremiumActiveAsync(string userId)
        {
            var premium = await GetByUserIdAsync(userId);
            return premium != null && premium.ExpireDate > DateTime.UtcNow;
        }

        public async Task RemoveExpiredAsync()
        {
            var expired = _context.UserPremiums
                .Where(x => x.ExpireDate <= DateTime.UtcNow);
            _context.UserPremiums.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }
        public async Task<List<UserPremium>> GetAllExpiredAsync()
        {
            return await _context.UserPremiums
                .Where(x => x.ExpireDate <= DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<List<UserPremium>> GetAllActiveAsync()
        {
            return await _context.UserPremiums
                .Where(x => x.ExpireDate > DateTime.UtcNow)
                .Include(x => x.User) // cần Include để lấy Username
                .ToListAsync();
        }

    }

}
