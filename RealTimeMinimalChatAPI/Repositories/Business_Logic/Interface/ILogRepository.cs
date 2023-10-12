using Microsoft.AspNetCore.Mvc;
using RealTimeMinimalChatAPI.Models.Domain;
using RealTimeMinimalChatAPI.Models.DTO;

namespace RealTimeMinimalChatAPI.Repositories.Business_Logic.Interface
{
    public interface ILogRepository
    {
        Task<IActionResult> GetLogsAsync(LogQueryParameters queryParameters);
    }
}
