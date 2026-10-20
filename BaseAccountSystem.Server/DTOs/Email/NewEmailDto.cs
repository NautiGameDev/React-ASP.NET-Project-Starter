namespace BaseAccountSystem.Server.DTOs.Email
{
    public class NewEmailDto
    {
        public string? Subject { get; set; }
        public string? ToUser { get; set; }
        public string? ToEmail { get; set; }
        public string? HtmlBody { get; set; }
    }
}
