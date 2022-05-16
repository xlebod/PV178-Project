using System.ComponentModel.DataAnnotations;

namespace SettleDown.Models
{
    public class SettleDownDebt
    {
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }
        [Required]
        public int? GroupId { get; set; }
        [Required]
        public SettleDownGroup Group { get; set; }
        [Required]
        public int? MemberId { get; set; }
        [Required]
        public SettleDownMember Member { get; set; }
    }
}
