using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealTimeMinimalChatAPI.Models.Domain;
using RealTimeMinimalChatAPI.Models.DTO;
using RealTimeMinimalChatAPI.Repositories.Business_Logic.Interface;
using RealTimeMinimalChatAPI.Repositories.Data_Access.Interface;

namespace RealTimeMinimalChatAPI.Repositories.Business_Logic.Implementation
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IDataAccessRepository dataRepository;
        public AuthRepository(UserManager<ApplicationUser> userManager,
            IDataAccessRepository dataRepository)
        {
            this.userManager = userManager;
            this.dataRepository = dataRepository;
           
        }

        public async Task<IActionResult> LoginAsync(LoginRequestDto request)
        {
            var tokenResponse = await dataRepository.LoginUserAsync(request);

            if (tokenResponse != null && tokenResponse.User != null)
            {
                var response = new LoginResponseDto
                {
                    Token = tokenResponse.Token,
                    Profile = new UserProfileDto
                    {
                        UserId = tokenResponse.User.Id,
                        Name = tokenResponse.User.UserName,
                        Email = tokenResponse.User.Email
                    }
                };

                return new OkObjectResult(response); // 200 OK
            }
            else if (tokenResponse != null && tokenResponse.IsSuccessful == false)
            {
                return new UnauthorizedResult(); // 401 Unauthorized
            }
            else
            {
                return new BadRequestObjectResult(new { error = "Login failed due to validation errors" }); // 400 Bad Request
            }
        }

        public async Task<IActionResult> GoogleLoginAsync(string credential)
        {
            var tokenResponse = await dataRepository.GoogleUserLoginAsync(credential);

            if (tokenResponse != null && tokenResponse.User != null)
            {
                var response = new LoginResponseDto
                {
                    Token = tokenResponse.Token,
                    Profile = new UserProfileDto
                    {

                        UserId = tokenResponse.User.Id,
                        Name = tokenResponse.User.UserName,
                        Email = tokenResponse.User.Email

                    }
                };

                return new OkObjectResult(response); // 200 OK
            }
            
            else
            {
                return new BadRequestObjectResult(new { error = "Google login failed" }); // 400 Bad Request
            }
        }


        public async Task<IActionResult> RegisterAsync(RegisterRequestDto request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Name,
                Email = request.Email
            };

            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return new ConflictObjectResult(new { error = "Email is already registered" });


            var identityResult = await dataRepository.RegisterUserAsync(user, request);

            if (identityResult.Succeeded)
            {
                var response = new RegisterResponseDto()
                {
                    UserId = user.Id,
                    Name = request.Name,
                    Email = request.Email

                };

                return new OkObjectResult( response); 

            }

            else return new BadRequestObjectResult(new { error = "Registration failed due to validation errors" });


        }
    }
}
