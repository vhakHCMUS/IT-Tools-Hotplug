using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using TKPM_Project.Models;
using TKPM_Project.Repositories;

namespace TKPM_Project.Controllers
{
    [Authorize]
    [Route("api/userlikedtools")]
    [ApiController]
    public class UserLikedToolController : ControllerBase
    {
        private readonly IUserLikedToolRepository _userLikedToolRepository;
        private readonly IToolRepository _toolRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserLikedToolController(
            IUserLikedToolRepository userLikedToolRepository,
            IToolRepository toolRepository,
            UserManager<ApplicationUser> userManager)
        {
            _userLikedToolRepository = userLikedToolRepository;
            _toolRepository = toolRepository;
            _userManager = userManager;
        }

        // Thích một công cụ
        [HttpPost("like")]
        public async Task<IActionResult> LikeTool([FromQuery] int toolId)
        {
            Console.WriteLine($"[LikeTool] User liking tool with ID: {toolId}");

            // Lấy ID của người dùng hiện tại
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User not authenticated");
                return Unauthorized("User not authenticated");
            }

            // Kiểm tra xem công cụ có tồn tại không
            var tool = await _toolRepository.GetByIdAsync(toolId);
            if (tool == null)
            {
                Console.WriteLine("Tool not found");
                return NotFound("Tool not found");
            }

            // Kiểm tra xem người dùng đã thích công cụ này chưa
            bool alreadyLiked = await _userLikedToolRepository.IsToolLikedAsync(userId, toolId);
            if (alreadyLiked)
            {
                Console.WriteLine("User already liked this tool");
                return BadRequest("You have already liked this tool");
            }

            // Thích công cụ
            await _userLikedToolRepository.LikeToolAsync(userId, toolId);

            Console.WriteLine("Tool liked successfully");
            return Ok($"You have liked tool {tool.Name}");
        }

        // Bỏ thích một công cụ
        [HttpPost("unlike")]
        public async Task<IActionResult> UnlikeTool([FromQuery] int toolId)
        {
            Console.WriteLine($"[UnlikeTool] User unliking tool with ID: {toolId}");

            // Lấy ID của người dùng hiện tại
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User not authenticated");
                return Unauthorized("User not authenticated");
            }

            // Kiểm tra xem công cụ có tồn tại không
            var tool = await _toolRepository.GetByIdAsync(toolId);
            if (tool == null)
            {
                Console.WriteLine("Tool not found");
                return NotFound("Tool not found");
            }

            // Kiểm tra xem người dùng đã thích công cụ này chưa
            bool alreadyLiked = await _userLikedToolRepository.IsToolLikedAsync(userId, toolId);
            if (!alreadyLiked)
            {
                Console.WriteLine("User has not liked this tool");
                return BadRequest("You have not liked this tool yet");
            }

            // Bỏ thích công cụ
            await _userLikedToolRepository.UnlikeToolAsync(userId, toolId);

            Console.WriteLine("Tool unliked successfully");
            return Ok($"You have unliked tool {tool.Name}");
        }

        // Kiểm tra xem người dùng đã thích công cụ chưa
        [HttpGet("isliked")]
        public async Task<IActionResult> IsToolLiked([FromQuery] int toolId)
        {
            Console.WriteLine($"[IsToolLiked] Checking if tool with ID {toolId} is liked");

            // Lấy ID của người dùng hiện tại
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User not authenticated");
                return Unauthorized("User not authenticated");
            }

            // Kiểm tra xem công cụ có tồn tại không
            var tool = await _toolRepository.GetByIdAsync(toolId);
            if (tool == null)
            {
                Console.WriteLine("Tool not found");
                return NotFound("Tool not found");
            }

            // Kiểm tra xem người dùng đã thích công cụ này chưa
            bool isLiked = await _userLikedToolRepository.IsToolLikedAsync(userId, toolId);

            Console.WriteLine($"Tool is liked: {isLiked}");
            return Ok(new { isLiked });
        }

        // Lấy danh sách công cụ mà người dùng đã thích
        [HttpGet("liked")]
        public async Task<IActionResult> GetLikedTools()
        {
            Console.WriteLine("[GetLikedTools] Retrieving liked tools");

            // Lấy ID của người dùng hiện tại
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User not authenticated");
                return Unauthorized("User not authenticated");
            }

            // Lấy danh sách công cụ đã thích
            var likedTools = await _userLikedToolRepository.GetLikedToolsAsync(userId);

            Console.WriteLine($"Found {likedTools.Count()} liked tools");
            return Ok(likedTools);
        }

        // API cho Admin - Lấy danh sách công cụ đã thích của một người dùng cụ thể
        [Authorize(Roles = "Admin")]
        [HttpGet("user")]
        public async Task<IActionResult> GetUserLikedTools([FromQuery] string userEmail)
        {
            Console.WriteLine($"[GetUserLikedTools] Retrieving liked tools for user: {userEmail}");

            // Tìm người dùng theo email
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                Console.WriteLine("User not found");
                return NotFound("User not found");
            }

            // Lấy danh sách công cụ đã thích của người dùng
            var likedTools = await _userLikedToolRepository.GetLikedToolsAsync(user.Id);

            Console.WriteLine($"Found {likedTools.Count()} liked tools for user {userEmail}");
            return Ok(likedTools);
        }
    }
}