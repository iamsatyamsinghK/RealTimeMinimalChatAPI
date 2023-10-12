using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealTimeMinimalChatAPI.Models.DTO;
using RealTimeMinimalChatAPI.Repositories.Business_Logic.Interface;

namespace RealTimeMinimalChatAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        
        private readonly IAuthRepository authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            
            this.authRepository = authRepository;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
                var response = await authRepository.RegisterAsync(request);
                return response;
            
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var response = await authRepository.LoginAsync(request);
            return response;
        }

        [HttpPost("LoginWithGoogle")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] string credential)
        {
            var response = await authRepository.GoogleLoginAsync(credential);
            return response;
        }


    }
}
