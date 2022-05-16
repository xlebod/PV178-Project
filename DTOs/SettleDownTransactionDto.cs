using System.ComponentModel.DataAnnotations;

namespace SettleDown.DTOs;

public class SettleDownTransactionDto
{
    [Required]
    public string Name { get; set; }
    [Required]
    public decimal Cost { get; set; }
    [Display(Name = "Who paid")]
    public int? MemberId { get; set; }
    [Required]
    [Display(Name = "Associated Group Id")]
    public int? GroupId { get; set; }

    public List<SettleDownDebtDto> Debts { get; set; } = new();
}