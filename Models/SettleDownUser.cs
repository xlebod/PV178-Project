using System.ComponentModel.DataAnnotations;

namespace SettleDown.Models
{
    public sealed class SettleDownUser
    {
        [Key]
        [Required]
        public string UserName { get; set; }
        [Required]
        public int? CredentialsId { get; set; }
        [Required]
        public SettleDownCredential Credentials { get; set; }
    }
}
