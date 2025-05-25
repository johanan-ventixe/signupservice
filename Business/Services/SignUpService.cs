using Business.Dtos;
using Data.Contexts;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;

namespace Business.Services;

public class SignUpService(DataContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration) : ISignUpService
{
    private readonly DataContext _context = context;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IConfiguration _configuration = configuration;

    public async Task<SignUpDto> SignUpForEventAsync(SignUpDto signUpDto)
    {
        try
        {
            /* Help from chatgpt how to communicate between SignUpService and EventDetailsService */
            var httpClient = _httpClientFactory.CreateClient();
            var eventDetailsServiceUrl = _configuration["ServiceUrls:EventDetailsService"];

            var ticketsResponse = await httpClient.GetAsync(
                $"{eventDetailsServiceUrl}/api/eventdetails/event/{signUpDto.EventId}/tickets");

            if (!ticketsResponse.IsSuccessStatusCode)
            {
                throw new Exception("Failed to verify ticket availability");
            }

            var ticketsLeft = await ticketsResponse.Content.ReadFromJsonAsync<int>();

            if (ticketsLeft <= 0)
            {
                throw new Exception("No tickets available for this event");
            }

            var signUpEntity = new SignUpEntity
            {
                EventId = signUpDto.EventId,
                FirstName = signUpDto.FirstName,
                LastName = signUpDto.LastName,
                Email = signUpDto.Email,
                PhoneNumber = signUpDto.PhoneNumber,
                SignUpDate = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")
                )
            };

            await _context.SignUps.AddAsync(signUpEntity);
            await _context.SaveChangesAsync();

            var updateResponse = await httpClient.PatchAsync(
                $"{eventDetailsServiceUrl}/api/eventdetails/event/{signUpDto.EventId}/tickets/{ticketsLeft - 1}",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            if (!updateResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to update tickets count: {updateResponse.StatusCode}");
            }

            return MapToSignUpsDto(signUpEntity);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to register for event: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<SignUpDto>> GetSignUpsByEventIdAsync(string eventId)
    {
        try
        {
            var signups = await _context.SignUps
                .Where(r => r.EventId == eventId)
                .ToListAsync();

            return signups.Select(MapToSignUpsDto);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get registrations for event ID {eventId}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<SignUpDto>> GetSignUpsByEmailAsync(string email)
    {
        try
        {
            var signups = await _context.SignUps
                .Where(r => r.Email == email)
                .ToListAsync();

            return signups.Select(MapToSignUpsDto);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get sign up for email {email}: {ex.Message}", ex);
        }
    }

    public async Task<SignUpDto?> GetSignUpsByIdAsync(string id)
    {
        try
        {
            var signups = await _context.SignUps.FindAsync(id);
            if (signups == null) return null;

            return MapToSignUpsDto(signups);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get sign up with ID {id}: {ex.Message}", ex);
        }
    }

    public async Task CancelSignUpAsync(string signUpId)
    {
        try
        {
            var signup = await _context.SignUps.FindAsync(signUpId);
            if (signup == null)
            {
                throw new Exception($"Registration with ID {signUpId} not found");
            }

            // First, fetch current tickets left
            var httpClient = _httpClientFactory.CreateClient();
            var eventDetailsServiceUrl = _configuration["ServiceUrls:EventDetailsService"];

            var ticketsResponse = await httpClient.GetAsync(
                $"{eventDetailsServiceUrl}/api/eventdetails/event/{signup.EventId}/tickets");

            if (ticketsResponse.IsSuccessStatusCode)
            {
                var ticketsLeft = await ticketsResponse.Content.ReadFromJsonAsync<int>();

                // Increment tickets available
                await httpClient.PatchAsync(
                    $"{eventDetailsServiceUrl}/api/eventdetails/event/{signup.EventId}/tickets/{ticketsLeft + 1}",
                    new StringContent(string.Empty, Encoding.UTF8, "application/json"));
            }

            _context.SignUps.Remove(signup);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to cancel sign up: {ex.Message}", ex);
        }
    }

    private static SignUpDto MapToSignUpsDto(SignUpEntity entity)
    {
        return new SignUpDto
        {
            Id = entity.Id,
            EventId = entity.EventId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
            SignUpDate = entity.SignUpDate
        };
    }
}