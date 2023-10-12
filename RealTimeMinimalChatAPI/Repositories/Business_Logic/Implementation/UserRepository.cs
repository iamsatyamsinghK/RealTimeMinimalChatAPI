using System;
using System.Security.Claims;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealTimeMinimalChatAPI.Exception;
using RealTimeMinimalChatAPI.Hubs;
using RealTimeMinimalChatAPI.Models.Domain;
using RealTimeMinimalChatAPI.Models.DTO;
using RealTimeMinimalChatAPI.Repositories.Business_Logic.Interface;
using RealTimeMinimalChatAPI.Repositories.Data_Access.Interface;

namespace RealTimeMinimalChatAPI.Repositories.Business_Logic.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly IDataAccessRepository dataRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
       
        private readonly static connection<string> _connections = new connection<string>();
        private readonly IHubContext<ChatHub> _hubContext;

        public UserRepository(IDataAccessRepository dataRepository, IHttpContextAccessor httpContextAccessor, IHubContext<ChatHub> chatHubContext, IHubContext<ChatHub> hubContext)
        {
            _httpContextAccessor = httpContextAccessor;
            this.dataRepository = dataRepository;
            _hubContext = hubContext;

        }

        public async Task<IActionResult> DeleteMessageAsync(int MessageId)
        {
            var ExtractedId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userId = ExtractedId;

            var message = await dataRepository.DeleteUserMessageAsync(MessageId);

            if (message == null)
            {
                return new NotFoundObjectResult(new { error = "Message not found" });
            }

            if (message.SenderId != userId)
            {
                return new UnauthorizedResult();
            }


            return new OkObjectResult(new { message = "Message deleted successfully" });
        }

        public async Task<IActionResult> EditMessageAsync(EditMessageRequestDto request)
        {
            var extractedId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userId = extractedId;

            if (request == null || string.IsNullOrWhiteSpace(request.Content))
            {
                return new BadRequestObjectResult(new { error = "Message content is required" });
            }

            var message = new Message
            {
                Id = request.MessageId,
                Content = request.Content,
            };

            try
            {
                message = await dataRepository.EditUserMessageAsync(message);

                if (message == null)
                {
                    return new NotFoundObjectResult(new { error = "Message not found" });
                }

                if (message.SenderId != userId)
                {
                    return new UnauthorizedResult();
                }

                return new OkObjectResult(new { message = "Message edited successfully" });
            }
            catch (NotFoundException ex)
            {
                return new NotFoundObjectResult(new { error = ex.Message });
            }
        }



        public async Task<IActionResult> GetListAsync()
        {
            var callingUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(callingUserId))
            {
                return new UnauthorizedResult(); // 401 Unauthorized
            }

            var users = await dataRepository.GetUserListAsync(callingUserId);

            if (users == null)
            {
                return new BadRequestObjectResult(new { error = "User List not found" }); // Return an appropriate message for a 404 Not Found
            }

            var response = users.Select(user => new UserProfileDto
            {
                UserId = user.Id,
                Name = user.UserName,
                Email = user.Email
            }).ToList();

            return new OkObjectResult(response);// 200 OK
        }

       

        public async Task<IActionResult> SearchMessageAsync(SearchMessageRequestDto request)
        {
            try
            {
                // Get the user's ID from the claims
                var ExtractedId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(ExtractedId))
                {
                    return new UnauthorizedResult();
                }

                if (request == null || string.IsNullOrWhiteSpace(request.query))
                {
                    return new BadRequestObjectResult(new { error = "Invalid request parameter" });
                }

                // Search for messages in conversations where the user is either the sender or receiver
                var conversations = await dataRepository.SearchUserMessageAsync(ExtractedId, request);

                if (conversations == null || conversations.Count() == 0)
                {
                    return new OkObjectResult(new { message = Array.Empty<ConversationHistoryResponseDto>() });

                }

                conversations = conversations.OrderByDescending(c => c.Timestamp);

                // Map the conversation history to the response DTO
                var response = conversations.Select(c => new ConversationHistoryResponseDto
                {

                    Id = c.Id,
                    SenderId = c.SenderId,
                    ReceiverId = c.Receivers.Any(r => r.ReceiverId == ExtractedId) ? ExtractedId : c.SenderId,
                    Content = c.Content,
                    Timestamp = c.Timestamp
                }).ToList();


                return new OkObjectResult(response);
            }
            catch (BadRequestException ex)
            {
                return new BadRequestObjectResult(new { error = ex.Message });
            }

        }



            public async Task<IActionResult> SendMessageAsync(SendMessageRequestCollectiveDto request)
            {

                if (request == null || string.IsNullOrWhiteSpace(request.Content))
                {
                    return new BadRequestObjectResult(new { error = "User List not found" });
                }


                var ExtractedId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var newMessage = new Message
                {
                    SenderId = ExtractedId,
                  
                    Content = request.Content,
                    Timestamp = DateTime.Now
                };



                try
                {
                    await dataRepository.SendUserMessageAsync(newMessage, request);

               

                var response = new SendMessageResponseCollectiveDto
                    {
                        MessageId = newMessage.Id,
                        SenderId = newMessage.SenderId,
                        ReceiverIds = request.ReceiverIds,
                        Content = newMessage.Content,
                        Timestamp = newMessage.Timestamp
                    };

                foreach (var receiverId in request.ReceiverIds)
                {
                    foreach (var connectionId in _connections.GetConnections(receiverId))
                    {
                        await _hubContext.Clients.Client(connectionId).SendAsync("BroadCast", response);
                    }
                }




                return new OkObjectResult(response);
                }
                catch (NotFoundException ex)
                {
                    return new NotFoundObjectResult(new { error = ex.Message });
                }


            }

        public async Task<IActionResult>ISendMessageAsync(SendMessageRequestDto request)
        {

            if (request == null || string.IsNullOrWhiteSpace(request.Content))
            {
                return new BadRequestObjectResult(new { error = "User List not found" });
            }


            var ExtractedId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var senderName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;

            var newMessage = new Message
            {
                SenderId = ExtractedId,

                Content = request.Content,
                Timestamp = DateTime.Now
            };



            try
            {
                await dataRepository.ISendUserMessageAsync(newMessage, request);



                var response = new SendMessageResponseDto
                {
                    MessageId = newMessage.Id,
                    SenderId = newMessage.SenderId,
                    SenderName = senderName,
                    ReceiverId = request.ReceiverId,
                    Content = newMessage.Content,
                    Timestamp = newMessage.Timestamp
                };

               
                    foreach (var connectionId in _connections.GetConnections(response.ReceiverId))
                    {
                        await _hubContext.Clients.Client(connectionId).SendAsync("BroadCast", response);
                    }
               




                return new OkObjectResult(response);
            }
            catch (NotFoundException ex)
            {
                return new NotFoundObjectResult(new { error = ex.Message });
            }



        }

            public async Task<IActionResult> SendMessageToNewChatAsync(SendMessageToNewChatRequestDto request)
            {
                var senderId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (request == null || request.ReceiverIds == null || !request.ReceiverIds.Any() || string.IsNullOrWhiteSpace(request.Content))
                {
                    return new BadRequestObjectResult(new { error = "Invalid request" });
                }

            try
            {
                var message = await dataRepository.SendMessageToNewUserChatAsync(senderId, request);

                var chatUsers = message.Chat.ChatUsers.Select(chatUser => chatUser.User);
                var receiverNames = chatUsers.Select(user => user.UserName).ToList();

                var response = new SendMessageToNewChatResponseDto
                {
                    MessageId = message.Id,
                    SenderId = message.SenderId,
                    ReceiverIds = request.ReceiverIds,
                    Content = request.Content,
                    Timestamp = message.Timestamp,
                    ChatId = message.Chat.Id,
                    Receivers = receiverNames
                };

                foreach (var receiverId in request.ReceiverIds)
                {
                    var receiverConnectionIds = _connections.GetConnections(receiverId).Except(new[] { senderId });
                    foreach (var connectionId in receiverConnectionIds)
                    {
                        await _hubContext.Clients.Client(connectionId).SendAsync("Group", response);
                    }
                }

                // Get the updated groups from your data repository
                var updatedGroup = await dataRepository.GetUpdatedGroupAsync(message.Chat.Id);

                var groupInfoDto = new GroupInfoDto
                {
                    ChatId = updatedGroup.Id,
                    Receivers = updatedGroup.ChatUsers
                        .Select(chatUser => chatUser.User.UserName)
                        .ToList()
                };


                var involvedUsersConnectionIds = request.ReceiverIds.SelectMany(receiverId => _connections.GetConnections(receiverId).Except(new[] { senderId })).ToList();

                    foreach (var connectionId in involvedUsersConnectionIds)
                    {
                        await _hubContext.Clients.Client(connectionId).SendAsync("UpdatedGroups", groupInfoDto);
                    }


                    return new OkObjectResult(response);

                
            }

            catch (NotFoundException ex)
            {
                return new BadRequestObjectResult(new { error = ex.Message });
            }
            }



        public async Task<IActionResult> GetConversationAsync(GetConversationRequestDto request)
        {
            if (request == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid request parameter" });
            }

            var extractedId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(extractedId))
            {
                return new UnauthorizedResult(); // 401 Unauthorized
            }



            var conversations = await dataRepository.GetUserConversationAsync(extractedId, request);

            if (request.Before.HasValue)
            {
                conversations = conversations.Where(chat =>
                    chat.Messages.OrderByDescending(message => message.Timestamp).FirstOrDefault()?.Timestamp < request.Before.Value);
            }
            else
            {
                conversations = conversations.Where(chat =>
                    chat.Messages.OrderByDescending(message => message.Timestamp).FirstOrDefault()?.Timestamp <= DateTime.Now);
            }


            if (!conversations.Any())
            {
                return new NotFoundObjectResult(new { error = "User or conversation not found" }); // 404 Not Found
            }

            // Apply sorting
            if (request.Sort.ToLower() == "desc")
            {
                conversations = conversations.OrderByDescending(chat =>
                    chat.Messages.OrderByDescending(message => message.Timestamp).FirstOrDefault()?.Timestamp);
            }
            else
            {
                conversations = conversations.OrderBy(chat =>
                    chat.Messages.OrderByDescending(message => message.Timestamp).FirstOrDefault()?.Timestamp);
            }

            // Limit the number of messages to be retrieved
            conversations = conversations.Take(request.Count);


            var response = new List<GetConversationResponseDto>();

            foreach (var chat in conversations)
            {
                foreach (var message in chat.Messages)
                {
                    var receiverIds = chat.ChatUsers.Select(cu => cu.UserId).ToList();
                    receiverIds.Remove(extractedId);

                    response.Add(new GetConversationResponseDto
                    {
                        MessageId = message.Id,
                        SenderId = message.SenderId,
                        ReceiverIds = receiverIds,
                        Content = message.Content,
                        Timestamp = message.Timestamp,
                        ChatId = chat.Id // You can set the ChatId to any appropriate value from your model
                    });
                }
            }

            return new OkObjectResult(response);
        }




        public async Task<IActionResult> GetConversationHistoryAsync(ConversationHistoryRequestDto request)
        {
            if (request == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid request parameter" });
            }

            var extractedId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(extractedId))
            {
                return new UnauthorizedResult(); // 401 Unauthorized
            }

           

            var conversations = await dataRepository.GetUserConversationHistoryAsync(extractedId, request);

            if (request.Before.HasValue)
            {
                conversations = conversations.Where(m => m.Timestamp < request.Before.Value);
            }
            else
            {
                conversations = conversations.Where(m => m.Timestamp <= DateTime.Now);
            }

            if (!conversations.Any())
            {
                return new NotFoundObjectResult(new { error = "User or conversation not found" }); // 404 Not Found
            }

            // Apply sorting
            if (request.Sort.ToLower() == "desc")
            {
                conversations = conversations.OrderByDescending(m => m.Timestamp);
            }
            else
            {
                conversations = conversations.OrderBy(m => m.Timestamp);
            }

            // Limit the number of messages to be retrieved
            conversations = conversations.Take(request.Count);

            var response = new List<ConversationHistoryResponseDto>();

            foreach (var conversation in conversations)
            {
                response.Add(new ConversationHistoryResponseDto
                {
                    Id = conversation.Id,
                    SenderId = conversation.SenderId,
                    ReceiverId = conversation.Receivers.FirstOrDefault()?.ReceiverId,
                    Content = conversation.Content,
                    Timestamp = conversation.Timestamp
                });
            }

            return new OkObjectResult(response);
        }

        public async Task<IActionResult> GetGroupsAsync()
        {
            var callingUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(callingUserId))
            {
                return new UnauthorizedResult(); // 401 Unauthorized
            }

            var groups = await dataRepository.GetUserGroupsAsync(callingUserId);

            if (groups == null || !groups.Any())
            {
                return new BadRequestObjectResult(new { error = "group list not found" }); // Return an appropriate message for a 404 Not Found
            }

            var groupInfoDtos = groups.Select(chat => new GroupInfoDto
            {
                ChatId = chat.Id,
                Receivers = chat.ChatUsers
                    .Select(chatUser => chatUser.User.UserName)
                    .ToList()
            }).ToList();



            // await _hubContext.Clients.All.SendAsync("UpdatedGroups", groupInfoDtos);


            return new OkObjectResult(groupInfoDtos); // 200 O
        }


    }
}
