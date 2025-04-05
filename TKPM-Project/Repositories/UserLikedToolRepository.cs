using Microsoft.EntityFrameworkCore;
using TKPM_Project.Models;

namespace TKPM_Project.Repositories
{
    public class UserLikedToolRepository : IUserLikedToolRepository
    {
        private readonly ApplicationDbContext _context;

        public UserLikedToolRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LikeToolAsync(string userId, int toolId)
        {
            if (!await _context.UserLikedTools.AnyAsync(x => x.UserId == userId && x.ToolId == toolId))
            {
                _context.UserLikedTools.Add(new UserLikedTool
                {
                    UserId = userId,
                    ToolId = toolId
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnlikeToolAsync(string userId, int toolId)
        {
            var liked = await _context.UserLikedTools
                .FirstOrDefaultAsync(x => x.UserId == userId && x.ToolId == toolId);

            if (liked != null)
            {
                _context.UserLikedTools.Remove(liked);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsToolLikedAsync(string userId, int toolId)
        {
            return await _context.UserLikedTools
                .AnyAsync(x => x.UserId == userId && x.ToolId == toolId);
        }

        public async Task<IEnumerable<Tool>> GetLikedToolsAsync(string userId)
        {
            return await _context.UserLikedTools
                .Where(x => x.UserId == userId)
                .Select(x => x.Tool)
                .ToListAsync();
        }
    }

}
