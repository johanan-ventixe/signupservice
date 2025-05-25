using Business.Dtos;
using Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignUpController(ISignUpService signUpService) : ControllerBase
    {
        private readonly ISignUpService _signUpService = signUpService;

        [HttpPost]
        public async Task<ActionResult<SignUpDto>> SignUpForEvent(SignUpDto signUpDto)
        {
            try
            {
                var signup = await _signUpService.SignUpForEventAsync(signUpDto);
                return CreatedAtAction(nameof(GetSignUp), new { id = signup.Id }, signup);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("No tickets available"))
                {
                    return BadRequest(ex.Message);
                }
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SignUpDto>> GetSignUp(string id)
        {
            try
            {
                var signup = await _signUpService.GetSignUpsByIdAsync(id);
                if (signup == null)
                {
                    return NotFound($"Sign up with ID {id} not found");
                }
                return Ok(signup);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<IEnumerable<SignUpDto>>> GetSignUpsByEvent(string eventId)
        {
            try
            {
                var signup = await _signUpService.GetSignUpsByEventIdAsync(eventId);
                return Ok(signup);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/{email}")]
        public async Task<ActionResult<IEnumerable<SignUpDto>>> GetRegistrationsByEmail(string email)
        {
            try
            {
                var signup = await _signUpService.GetSignUpsByEmailAsync(email);
                return Ok(signup);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelSignUp(string id)
        {
            try
            {
                await _signUpService.CancelSignUpAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found"))
                {
                    return NotFound(ex.Message);
                }
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
