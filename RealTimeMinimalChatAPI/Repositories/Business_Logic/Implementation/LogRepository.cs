using Microsoft.AspNetCore.Mvc;
using RealTimeMinimalChatAPI.Models.Domain;
using RealTimeMinimalChatAPI.Models.DTO;
using RealTimeMinimalChatAPI.Repositories.Business_Logic.Interface;
using RealTimeMinimalChatAPI.Repositories.Data_Access.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealTimeMinimalChatAPI.Repositories.Business_Logic.Implementation
{
    public class LogRepository : ILogRepository
    {
        private readonly IDataAccessRepository dataRepository;

        public LogRepository(IDataAccessRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        public async Task<IActionResult> GetLogsAsync(LogQueryParameters queryParameters)
        {
            try
            {
                // Get the Indian Standard Time (IST) timezone
                TimeZoneInfo istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                // Convert the current time to IST
                DateTime currentTimeUtc = DateTime.UtcNow;
                DateTime currentTimeIST = TimeZoneInfo.ConvertTimeFromUtc(currentTimeUtc, istTimeZone);

                // Calculate the default start time (Current Timestamp - 5 minutes)
                DateTime defaultStartTimeIST = currentTimeIST.AddMinutes(-120);

                // Convert the start time and end time to IST, if provided
                DateTime startTimeIST = queryParameters.StartTime.HasValue
                    ? TimeZoneInfo.ConvertTimeFromUtc(queryParameters.StartTime.Value, istTimeZone)
                    : defaultStartTimeIST;

                DateTime endTimeIST = queryParameters.EndTime.HasValue
                    ? TimeZoneInfo.ConvertTimeFromUtc(queryParameters.EndTime.Value, istTimeZone)
                    : currentTimeIST;

                var logs = await dataRepository.GetUserLogsAsync(startTimeIST, endTimeIST);

                if (logs == null || logs.Count() == 0)
                {
                    return new NotFoundObjectResult(new { error = "Logs not found" }); // 404 Not Found
                }
                var response = new List<LogDto>();

                foreach (var log in logs)
                {
                    response.Add(new LogDto
                    {
                        IpAddress = log.IpAddress,
                        Username = log.Username,

                        RequestBody = log.RequestBody,
                        Timestamp = log.Timestamp
                    });
                }

                return new OkObjectResult(response); // 200 OK - Log list received successfully
            }
            catch
            {
                return new BadRequestObjectResult(new { error = "Invalid request parameters" }); // 400 Bad Request - Invalid request parameters
            }
        }
    }
}
