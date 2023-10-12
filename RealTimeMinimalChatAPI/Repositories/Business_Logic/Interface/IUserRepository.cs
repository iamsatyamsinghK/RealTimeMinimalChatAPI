using Microsoft.AspNetCore.Mvc;
using RealTimeMinimalChatAPI.Models.Domain;
using RealTimeMinimalChatAPI.Models.DTO;

namespace RealTimeMinimalChatAPI.Repositories.Business_Logic.Interface
{
    public interface IUserRepository
    {
        Task<IActionResult> GetListAsync();
        Task<IActionResult> GetGroupsAsync();
        Task<IActionResult> SendMessageAsync(SendMessageRequestCollectiveDto request);
        Task<IActionResult>ISendMessageAsync(SendMessageRequestDto request);
        Task<IActionResult> SendMessageToNewChatAsync(SendMessageToNewChatRequestDto request);

        Task<IActionResult> EditMessageAsync(EditMessageRequestDto request);
        Task<IActionResult> DeleteMessageAsync(int MessageId);
        Task<IActionResult> GetConversationHistoryAsync(ConversationHistoryRequestDto request);
        Task<IActionResult> SearchMessageAsync(SearchMessageRequestDto request);
        Task<IActionResult> GetConversationAsync(GetConversationRequestDto request);



    }
}
