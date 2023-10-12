using Microsoft.AspNetCore.Mvc;
using RealTimeMinimalChatAPI.Models.DTO;

namespace RealTimeMinimalChatAPI.Repositories.Business_Logic.Interface
{
    public interface IAuthRepository
    {
        Task<IActionResult> RegisterAsync(RegisterRequestDto request);
        Task<IActionResult> LoginAsync(LoginRequestDto request);
        Task<IActionResult> GoogleLoginAsync(string credential);
    }
}
