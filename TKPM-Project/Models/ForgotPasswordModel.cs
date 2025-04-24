using System.ComponentModel.DataAnnotations;

namespace TKPM_Project.Models
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }
    }
} 