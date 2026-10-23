namespace BaseAccountSystem.Server.DTOs.Email
{
    public class PasswordResetDto
    {
        public string? UserId { get; set; }
        public string? Code { get; set; }
        public string? NewPassword { get; set; }
    }
}
