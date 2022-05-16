using System.ComponentModel.DataAnnotations;

namespace SettleDown.DTOs;

public class SettleDownMemberPutDto
{
    [Required]
    public string Name { get; set; }
}