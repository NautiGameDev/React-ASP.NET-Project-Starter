namespace BaseAccountSystem.Server.DTOs.API
{
    public class AuthResultDto
    {
        public bool Success { get; set; } = false;
        public string? Token { get; set; } = "";
        public string? RefreshToken { get; set; } = "";
        public string? ErrorMessage { get; set; } = "";
        public AuthErrorType ErrorType { get; set; } = AuthErrorType.ProcessFailed;
        public string? UserId { get; set; } = "Unknown";
    }

    public enum AuthErrorType { None, NotFound, InvalidCredentials, ProcessFailed }
}
