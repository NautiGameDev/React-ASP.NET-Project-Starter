namespace BaseAccountSystem.Server.DTOs.Account
{
    public class UpdateAccountDto
    {
        public string? Email { get; set; } = "";
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; } = "";
    }
}
