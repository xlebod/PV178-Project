using System.ComponentModel.DataAnnotations;

namespace SettleDown.DTOs;

public class SettleDownDebtDto
{
    public int? Weight { get; set; }
    public decimal? Amount { get; set; }
    [Required]
    [Display(Name = "Paid for:")]
    public int? MemberId { get; set; }
}