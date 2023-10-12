using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealTimeMinimalChatAPI.Models.Domain;
using RealTimeMinimalChatAPI.Models.DTO;

namespace RealTimeMinimalChatAPI.Repositories.Data_Access.Interface
{
    public interface IDataAccessRepository
    {
        Task<IdentityResult> RegisterUserAsync(ApplicationUser user, RegisterRequestDto request);
        Task<TokenResponseDto> LoginUserAsync(LoginRequestDto request);
        Task<List<ApplicationUser>> GetUserListAsync(string callingUserId);
        Task<List<Chat>> GetUserGroupsAsync(string callingUserId);
        Task<Message> SendUserMessageAsync(Message newMessage, SendMessageRequestCollectiveDto request);
        Task<Message> EditUserMessageAsync(Message newMessage);
        Task<Message> DeleteUserMessageAsync(int MessageId);
        Task<IEnumerable<Message>> GetUserConversationHistoryAsync(string ExtractedId, ConversationHistoryRequestDto request);
        Task<IEnumerable<Chat>> GetUserConversationAsync(string ExtractedId, GetConversationRequestDto request);
        Task<List<Log>> GetUserLogsAsync(DateTime startTimeIST, DateTime endTimeIST);
        Task<IEnumerable<Message>> SearchUserMessageAsync( string ExtractedId, SearchMessageRequestDto request);
        Task<TokenResponseDto> GoogleUserLoginAsync(string credential);
        Task<Message>ISendUserMessageAsync(Message newMessage, SendMessageRequestDto request);
        Task<Message> SendMessageToNewUserChatAsync(string senderId, SendMessageToNewChatRequestDto request);
        Task<Chat> GetUpdatedGroupAsync(int chatId);


    }
}
