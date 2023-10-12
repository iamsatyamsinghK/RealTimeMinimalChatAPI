using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealTimeMinimalChatAPI.Models.Domain;
using RealTimeMinimalChatAPI.Models.DTO;
using RealTimeMinimalChatAPI.Repositories.Business_Logic.Implementation;
using RealTimeMinimalChatAPI.Repositories.Business_Logic.Interface;

namespace RealTimeMinimalChatAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserCRUDController : ControllerBase
    {
        private readonly IUserRepository userRepository;

        public UserCRUDController(IUserRepository userRepository)
        {

            this.userRepository = userRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserList()
        {

            var response = await userRepository.GetListAsync();
            return response;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestCollectiveDto request)
        {
            var response = await userRepository.SendMessageAsync(request);
            return response;
        }

        [HttpPost]
       [Route("messages")]
        [Authorize]
        public async Task<IActionResult>ISendMessage([FromBody] SendMessageRequestDto request)
        {
            var response = await userRepository.ISendMessageAsync(request);
            return response;
        }

        [HttpPost]
        [Route("send-message-to-new-chat")]
        [Authorize]
        public async Task<IActionResult> SendMessageToNewChat([FromBody] SendMessageToNewChatRequestDto request)
        {
            var response = await userRepository.SendMessageToNewChatAsync(request);
            return response;
        }

        [HttpGet]
        [Route("get-conversation")]
        [Authorize]
        public async Task<IActionResult> GetConversation([FromQuery] GetConversationRequestDto request)
        {
            var response = await userRepository.GetConversationAsync(request);
            return response;
        }

        [HttpGet]
        [Route("get-group")]
        [Authorize]
        public async Task<IActionResult> GetGroups()
        {

            var response = await userRepository.GetGroupsAsync();
            return response;
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> EditMessage([FromBody] EditMessageRequestDto request)
        {
            var response = await userRepository.EditMessageAsync(request);
            return response;
        }

        [HttpDelete]
        [Route("{MessageId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteMessage([FromRoute] int MessageId)
        {
            var response = await userRepository.DeleteMessageAsync(MessageId);
            return response;
        }

        [HttpGet]
        [Route("messages")]
        [Authorize]
        public async Task<IActionResult> GetConversationHistory([FromQuery] ConversationHistoryRequestDto request)
        {
            var response = await userRepository.GetConversationHistoryAsync(request);
            return response;
        }



        [HttpGet]
        [Route("searchMessages")]
        [Authorize]
        public async Task<IActionResult> SearchMessage([FromQuery] SearchMessageRequestDto request)
        {
            var response = await userRepository.SearchMessageAsync(request);
            return response;
        }


    }

}
