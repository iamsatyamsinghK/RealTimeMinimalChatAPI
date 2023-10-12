using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Azure.Core;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RealTimeMinimalChatAPI.Data;
using RealTimeMinimalChatAPI.Exception;
using RealTimeMinimalChatAPI.Models.Domain;
using RealTimeMinimalChatAPI.Models.DTO;
using RealTimeMinimalChatAPI.Repositories.Data_Access.Interface;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RealTimeMinimalChatAPI.Repositories.Data_Access.Implementation
{
    public class DataAccessRepository : IDataAccessRepository
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;
        public DataAccessRepository(UserManager<ApplicationUser> userManager, IConfiguration configuration, ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.dbContext = dbContext;
        }
        public async Task<IdentityResult> RegisterUserAsync(ApplicationUser user, RegisterRequestDto request)
        {

            var identityResult = await userManager.CreateAsync(user, request.Password);
            return identityResult;

        }

        public async Task<TokenResponseDto> LoginUserAsync(LoginRequestDto request)
        {
            var identityUser = await userManager.FindByEmailAsync(request.Email);

            if (identityUser is not null)
            {
                // Check Password
                var checkPasswordResult = await userManager.CheckPasswordAsync(identityUser, request.Password);
                if (checkPasswordResult)
                {
                    return await JwtToken(identityUser);
                }
                else
                {
                    return null; 
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<List<ApplicationUser>> GetUserListAsync(string callingUserId)
        {
            return await dbContext.Users.Where(u => u.Id != callingUserId).ToListAsync();
        }

        public async Task<Message> SendUserMessageAsync(Message newMessage, SendMessageRequestCollectiveDto request)
        {
            // Check if all receivers exist
            foreach (var receiverId in request.ReceiverIds)
            {
                var receiverExists = dbContext.Users.Any(r => r.Id == receiverId);
                if (!receiverExists)
                {
                    throw new NotFoundException($"Receiver with ID '{receiverId}' not found");
                }

                // Handle message receivers and add entries to MessageReceiver table
                var messageReceiver = new MessageReceiver
                {
                    Message = newMessage,
                    ReceiverId = receiverId
                };

                dbContext.MessageReceivers.Add(messageReceiver);
            }

            // Save changes to the database
            await dbContext.SaveChangesAsync();

            return newMessage;
        }


        public async Task<Message> EditUserMessageAsync(Message newMessage)
        {

            var existingMessage = await dbContext.Messages.FirstOrDefaultAsync(x => x.Id == newMessage.Id);
            if (existingMessage != null)
            {

                existingMessage.Content = newMessage.Content;
                await dbContext.SaveChangesAsync();

                return existingMessage;
            }

            return null;
        }

        public async Task<Message> DeleteUserMessageAsync(int MessageId)
        {
            var existingMessage = await dbContext.Messages.FirstOrDefaultAsync(x => x.Id == MessageId);
            if (existingMessage != null)
            {
                dbContext.Messages.Remove(existingMessage);
                await dbContext.SaveChangesAsync();
                return existingMessage;

            }

            return null;
        }

        public async Task<IEnumerable<Message>> GetUserConversationHistoryAsync(string ExtractedId, ConversationHistoryRequestDto request)
        {
            var conversation = await dbContext.Messages
                .Include(m => m.Receivers) // Include the Receivers navigation property
                .Where(m =>
                    (m.SenderId == ExtractedId && m.Receivers != null && m.Receivers.Any(r => r.ReceiverId == request.UserId)) ||
                    (m.SenderId == request.UserId && m.Receivers != null && m.Receivers.Any(r => r.ReceiverId == ExtractedId))
                )
                .ToListAsync();

            return conversation;
        }


        public async Task<IEnumerable<Chat>> GetUserConversationAsync(string ExtractedId, GetConversationRequestDto request)
        {
            var conversation = await dbContext.Chats
                .Where(chat =>(chat.Id==request.ChatId))

                .Include(chat => chat.ChatUsers)

                .Include(chat => chat.Messages)
                     // Include Chat for Sender

                
                .ToListAsync();


            return conversation;

        }

        public async Task<List<Log>> GetUserLogsAsync(DateTime startTimeIST, DateTime endTimeIST)
        {

            var logs = await dbContext.Logs
                .Where(log => log.Timestamp >= startTimeIST && log.Timestamp <= endTimeIST)
                .ToListAsync();

            return logs;
        }

        public async Task<IEnumerable<Message>> SearchUserMessageAsync(string ExtractedId, SearchMessageRequestDto request)
        {
            var query = request.query.ToLower(); // Convert the query to lowercase for case-insensitive search


            var conversations = await dbContext.Messages
                .Include(m => m.Receivers)
                .Where(m =>
                    (m.SenderId == ExtractedId || m.Receivers.Any(r => r.ReceiverId == ExtractedId)) && // User is sender or receiver
                    m.Content.ToLower().Contains(query)) // Convert the content to lowercase and check for the query
                .ToListAsync();

            return conversations;
        }

        public async Task<TokenResponseDto> GoogleUserLoginAsync(string credential)
        {
            var GoogleClientId = configuration["Jwt:GoogleClientId"]; // Retrieve Google client ID from configuration

            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string> { GoogleClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

            var Email = payload.Email; // Assuming the username is in the payload

            //var nameParts = fullName.Split(' ');
            //var firstName = nameParts[0]; // Get the first part as the first name

            // Look up the user using the first name
            var identityUser = await userManager.FindByEmailAsync(Email);

            if (identityUser != null)
            {
                return await JwtToken(identityUser);
            }
            else return null;

        }

        public Task<TokenResponseDto> JwtToken(ApplicationUser identityUser)
        {

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, identityUser.Email),
                new Claim(ClaimTypes.Name, identityUser.UserName),
                 new Claim(ClaimTypes.NameIdentifier, identityUser.Id)
                // You can add more claims as needed
            };

            // JWT Security Token Parameters
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(600),
                signingCredentials: credentials);

            // Create the DTO with the user and token
            var responseDto = new TokenResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                User = identityUser,
                IsSuccessful = true
            };

            return Task.FromResult(responseDto);
        }

        public async Task<Message> ISendUserMessageAsync(Message newMessage, SendMessageRequestDto request)
        {
            // Check if all receivers exist
            
                var receiverExists = dbContext.Users.Any(r => r.Id == request.ReceiverId);
                if (!receiverExists)
                {
                    throw new NotFoundException($"Receiver with ID '{request.ReceiverId}' not found");
                }

                // Handle message receivers and add entries to MessageReceiver table
                var messageReceiver = new MessageReceiver
                {
                    Message = newMessage,
                    ReceiverId = request.ReceiverId
                };

                dbContext.MessageReceivers.Add(messageReceiver);
            

            // Save changes to the database
            await dbContext.SaveChangesAsync();

            return newMessage;
        }

        public async Task<Message> SendMessageToNewUserChatAsync(string senderId, SendMessageToNewChatRequestDto request)
        {
            var chats = await dbContext.Chats
                .Include(c => c.ChatUsers)
                .ThenInclude(chatUser => chatUser.User)
                
                .ToListAsync();

            // Find a chat where the set of receivers exactly matches the request receivers
            var existingChat = chats.FirstOrDefault(c =>
                c.ChatUsers.Select(chatUser => chatUser.UserId).Concat(new[] { senderId }).ToHashSet().SetEquals(request.ReceiverIds.Concat(new[] { senderId })));


            if (existingChat != null)
            {
                // If an existing chat is found, add the message to the existing chat
                var newMessage = new Message
                {
                    SenderId = senderId,
                    Content = request.Content,
                    Timestamp = DateTime.Now
                };
                
                existingChat.Messages.Add(newMessage);
                await dbContext.SaveChangesAsync();
                return newMessage; 
            }
            else
            {
              var chat = new Chat
              {

                CreatorId = senderId,
                ChatUsers = request.ReceiverIds.Select(userId => new ChatUser { UserId = userId }).ToList(),
                Messages = new List<Message>
                {
                   new Message
                   {
                     SenderId = senderId,
                     Content = request.Content,
                     Timestamp = DateTime.Now
                   }
                }
              };
                chat.ChatUsers.Add(new ChatUser { UserId = senderId });
                dbContext.Chats.Add(chat);
                await dbContext.SaveChangesAsync();


                var newChat = await dbContext.Chats
                    .Include(c => c.ChatUsers)
                    .ThenInclude(chatUser => chatUser.User)
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == chat.Id);

                return newChat.Messages.First();
            }
        }


        public async Task<List<Chat>> GetUserGroupsAsync(string callingUserId)
        {
            var userGroups = await dbContext.Chats
                .Include(cu => cu.ChatUsers)
                .ThenInclude(chatUser => chatUser.User)
                 .Where(chat => chat.ChatUsers.Any(chatUser => chatUser.UserId == callingUserId))

                .ToListAsync(); 

            return userGroups;
        }

        public async Task<Chat> GetUpdatedGroupAsync(int chatId)
        {
            // Implement your logic to fetch the specific group by chatId
            // For example, assuming Chat has a property called Id for comparison:

            var group = await dbContext.Chats
                .Include(cu => cu.ChatUsers)
                .ThenInclude(chatUser => chatUser.User)
                .FirstOrDefaultAsync(chat => chat.Id == chatId);

            return group;
        }





    }
}

