using Business.Dtos;

namespace Business.Services;

public interface ISignUpService
{
    Task<SignUpDto> SignUpForEventAsync(SignUpDto signUpDto);
    Task<IEnumerable<SignUpDto>> GetSignUpsByEmailAsync(string email);
    Task<IEnumerable<SignUpDto>> GetSignUpsByEventIdAsync(string eventId);
    Task<SignUpDto?> GetSignUpsByIdAsync(string id);
    Task CancelSignUpAsync(string signUpId);
}