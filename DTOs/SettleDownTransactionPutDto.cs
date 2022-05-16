using System.ComponentModel.DataAnnotations;

namespace SettleDown.DTOs;

public class SettleDownTransactionPutDto
{
    [Required]
    public string Name { get; set; }
}