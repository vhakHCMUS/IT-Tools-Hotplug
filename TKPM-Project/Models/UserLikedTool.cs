namespace TKPM_Project.Models
{
    public class UserLikedTool
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int ToolId { get; set; }
        public Tool Tool { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }

}
