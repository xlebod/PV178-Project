using System.ComponentModel.DataAnnotations;

namespace SettleDown.Models
{
    public class SettleDownTransaction
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public decimal Cost { get; set; }
        [Required]
        public int? MemberId { get; set; }
        [Required]
        public SettleDownMember Member { get; set; }
        [Required]
        public int? GroupId { get; set; }
        [Required]
        public SettleDownGroup Group { get; set; }
    }
}
