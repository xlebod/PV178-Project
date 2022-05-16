using SettleDown.Models;

namespace SettleDown.DTOs;

public class SettleDownGroupDto
{
    public string Name { get; set; }
    public List<SettleDownMember> Members { get; set; } = new();
    public List<SettleDownDebt> Debts { get; set; } = new();
}