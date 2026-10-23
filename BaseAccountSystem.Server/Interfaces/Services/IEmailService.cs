using BaseAccountSystem.Server.DTOs.Email;

namespace BaseAccountSystem.Server.Interfaces.Services
{
    public interface IEmailService
    {
        public Task CreateEmail(NewEmailDto dto);
    }
}
