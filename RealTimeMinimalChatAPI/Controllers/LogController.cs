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
    public class LogController : ControllerBase
    {

        private readonly ILogRepository logRepository;

        public LogController(ILogRepository logRepository)
        {
            this.logRepository = logRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] LogQueryParameters queryParameters)
        {
            var response = await logRepository.GetLogsAsync(queryParameters);
            return response;
        }
    }
}

