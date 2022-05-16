using SettleDown.DTOs;
using SettleDown.Models;

namespace SettleDown.Services;

public interface ITokenService
{
    Task<string> GenerateToken(SettleDownUserDto user);
}