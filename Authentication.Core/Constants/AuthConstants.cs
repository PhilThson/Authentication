namespace Authentication.Core.Constants
{
    public static class AuthConstants
	{
        public const string UserIdClaim = "user_id";
        public const string TokenPolicy = "Token";
        public const string JwtKey = nameof(JwtKey);
        public const string CorsPolicy = "MyCorsPolicy";
        public const string RegistrationKey = nameof(RegistrationKey);
        public const string ApiKeyAuth = "ApiKey";
        public const string ApiKeyAuthPolicy = "ApiKeyPolicy";
    }
}

