using Microsoft.AspNetCore.Mvc;
using SettleDown.DTOs;

namespace SettleDown.Services;

public interface IDebtManagementService
{
    Task<ActionResult?> DisperseDebtFromTransaction(SettleDownTransactionDto transactionDto);
}