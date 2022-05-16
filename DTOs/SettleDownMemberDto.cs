using System.ComponentModel.DataAnnotations;

namespace SettleDown.DTOs;

public class SettleDownMemberDto
{
    [Required]
    public string Name { get; set; }
    [Required]
    public int? GroupId { get; set; }
    [Required]
    public string? UserId { get; set; }
}