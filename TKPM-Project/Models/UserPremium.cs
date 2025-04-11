namespace TKPM_Project.Models
{
    public class UserPremium
    {
        public int Id { get; set; }

        public string UserId { get; set; } // FK to AspNetUsers
        public ApplicationUser User { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime ExpireDate { get; set; }
    }

}
