using BookingServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _emailService.SendEmailWithImageUrlAsync(request.ToEmail, request.Subject, request.Body, request.ImageUrl);
            return Ok();
        }
    }

    public class EmailRequest
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ImageUrl { get; set; } 
    }
}
