namespace BaseAccountSystem.Server.Helpers
{
    public static class TokenHandler
    {
        public static DateTime GetAuthTokenExpiration()
        {
            return DateTime.UtcNow.AddMinutes(30);
        }

        public static DateTime GetRefreshTokenExpiration()
        {
            return DateTime.UtcNow.AddDays(7);
        }

    }
}
