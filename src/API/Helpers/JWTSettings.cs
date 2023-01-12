namespace API.Helpers
{
    public class JWTSettings
    {
        public string ValidAudience { get; set; }
        public string ValidIssuer { get; set; }
        public string Secret { get; set; }
        public string TokenValidityInMinutes { get; set; }
        public string RefreshTokenValidityInDays { get; set; }
    }
}
