namespace Authentication.Core.Settings
{
    public class JwtSettings
	{
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpirationTimeMin { get; set; }
        public int RefreshTokenExpirationTimeDays { get; set; } = 1;
    }
}

