using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SettleDown.CRUDs;
using SettleDown.Data;
using SettleDown.DTOs;
using SettleDown.Models;

namespace SettleDown.Services;

[NonController]
public class SettleDownDebtManagementService : ControllerBase, IDebtManagementService
{
    private readonly SettleDownContext _context;
    private readonly ICrudService<SettleDownDebt> _debtCrudService;

    public SettleDownDebtManagementService(SettleDownContext context, ICrudService<SettleDownDebt> debtCrudService)
    {
        _context = context;
        _debtCrudService = debtCrudService;
    }


    public async Task<ActionResult?> DisperseDebtFromTransaction(SettleDownTransactionDto transactionDto)
    {
        ActionResult? addBalanceToPayingMember = await AddBalanceToPayingMember(
            transactionDto.MemberId, transactionDto.GroupId, transactionDto.Cost);
        if (addBalanceToPayingMember != null)
            return addBalanceToPayingMember;
        
        if (transactionDto.Debts.Count == 0)
        {
            ActionResult? disperseDebtEquallyAmongGroup = 
                await DisperseDebtEquallyAmongGroup(transactionDto.GroupId, transactionDto.Cost);
            if (disperseDebtEquallyAmongGroup != null)
            {
                return disperseDebtEquallyAmongGroup;
            }
        }

        ActionResult? disperseDebtFromDebtDtos = await DisperseDebtFromDebtDtos(transactionDto);
        return disperseDebtFromDebtDtos ?? null;
    }

    private async Task<ActionResult?> DisperseDebtFromDebtDtos(SettleDownTransactionDto transactionDto)
    {
        ActionResult? convertWeightsIntoAmountsInDebtDtos = ConvertWeightsIntoAmountsInDebtDtos(transactionDto);
        if(convertWeightsIntoAmountsInDebtDtos != null)
        {
            return convertWeightsIntoAmountsInDebtDtos;
        }

        if (transactionDto.Cost < transactionDto.Debts.Select(d => d.Amount)
                .Aggregate((decimal) 0, (s, a) => (decimal) (s + a!)))
        {
            return BadRequest("Amounts of debt being assigned is higher than the total cost!");
        }

        foreach (SettleDownDebtDto debtDto in transactionDto.Debts)
        {
            ActionResult? actionResult = await CreateOrUpdateDebtForMember(transactionDto.GroupId, debtDto);
            if (actionResult != null)
                return actionResult;
        }
        return null;
    }

    private ActionResult? ConvertWeightsIntoAmountsInDebtDtos(SettleDownTransactionDto transactionDto)
    {
        if (transactionDto.Debts.Any(d => d.Amount != null && d.Weight != null))
            return BadRequest("Cannot define both Weight and Amount!");
        if (transactionDto.Debts.Any(d => d.Weight == 0))
            return BadRequest("Cannot input a zero value for Weight!");

        int totalWeight = CalculateTotalWeight(transactionDto);
        decimal costsToBeSplit = CalculateCostsToBeSplit(transactionDto);

        transactionDto.Debts = transactionDto.Debts
            .Select(d => new SettleDownDebtDto
                {
                    MemberId = d.MemberId,
                    Amount = d.Amount ?? costsToBeSplit / ((decimal) d.Weight! / totalWeight)
                })
            .ToList();
        return null;
    }

    private static decimal CalculateCostsToBeSplit(SettleDownTransactionDto transactionDto)
    {
        decimal? totalCostInAmountFields = transactionDto.Debts
            .Where(d => d.Amount != null)
            .Select(d => d.Amount)
            .Sum();
        return transactionDto.Cost - totalCostInAmountFields ?? 0;
    }

    private static int CalculateTotalWeight(SettleDownTransactionDto transactionDto)
    {
        int? totalWeight = transactionDto.Debts
            .Where(d => d.Weight != null)
            .Select(d => d.Weight).Sum()!;
        
        return totalWeight ?? 0;
    }

    private async Task<ActionResult?> AddBalanceToPayingMember(int? memberId, int? groupId, decimal cost)
    {
        var debtDto = new SettleDownDebtDto
        {
            Amount = -cost,
            MemberId = memberId
        };

        return await CreateOrUpdateDebtForMember(groupId, debtDto);
    }

    private async Task<ActionResult?> DisperseDebtEquallyAmongGroup(int? groupId, decimal cost)
    {
        if (_context.SettleDownMember == null)
            return Problem("Entity set 'SettleDownContext.SettleDownMember'  is null.");
        
        List<SettleDownMember> groupMembers = await _context.SettleDownMember
            .Where(m => m.GroupId == groupId).ToListAsync();

        foreach (SettleDownMember member in groupMembers)
        {
            var debtDto = new SettleDownDebtDto
            {
                MemberId = member.Id,
                Amount = decimal.Round(cost / (groupMembers.Count), 2)
            };
            
            ActionResult? actionResult = await CreateOrUpdateDebtForMember(groupId, debtDto);
            if (actionResult != null)
            {
                return actionResult;
            }
        }
        return null;
    }

    private async Task<ActionResult?> CreateOrUpdateDebtForMember(int? groupId, SettleDownDebtDto debtDto)
    {
        if (debtDto.Amount == null)
        {
            return BadRequest("You need to input either Weight or Amount!");
        }
        if (_context.SettleDownDebt == null)
            return Problem("Entity set 'SettleDownContext.SettleDownDebt'  is null.");
        
        if (await _context.SettleDownDebt.AnyAsync(d => d.GroupId == groupId && d.MemberId == debtDto.MemberId))
        {
            SettleDownDebt existingDebt = (await _context.SettleDownDebt
                .Where(d => d.GroupId == groupId && d.MemberId == debtDto.MemberId).ToListAsync())[0];
            existingDebt.Amount += (decimal) debtDto.Amount;
            if (!await _debtCrudService.UpdateWithoutSaving(existingDebt.Id.ToString(), existingDebt))
            {
                return Problem($"Encountered problem while updating debt Id: {existingDebt.Id}!");
            }
            return null;
        }
        
        var debt = new SettleDownDebt
        {
            Amount = (decimal) debtDto.Amount,
            GroupId = groupId,
            MemberId = debtDto.MemberId
        };
        
        return await _debtCrudService.CreateWithoutSaving(debt) != null ?
            null : Problem($"Encountered problem while creating debt for Member: {debtDto.MemberId}");
    }
}