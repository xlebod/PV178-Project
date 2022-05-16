using System.ComponentModel.DataAnnotations;

namespace SettleDown.Models
{
    public class SettleDownCredential
    {
        [Key] 
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public byte[] Salt { get; set; }
    }
}
