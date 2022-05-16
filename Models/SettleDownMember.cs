using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SettleDown.Models;

namespace SettleDown.Models
{
    public class SettleDownMember
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [DataType(DataType.Currency)]
        public decimal TotalBalance { get; set; } = 0;
        [ForeignKey("UserUserName")]
        public string? UserId { get; set; }
        [Required]
        public SettleDownUser User { get; set; }
        public int? GroupId { get; set; }
        [Required]
        public SettleDownGroup Group { get; set; }
    }
}
