namespace EmergencyManagement.Token
{
    public class JwtSettings
    {
        public string Secret { get; set; }=string.Empty;
        public int DurationInMinutes { get; set; } = 60;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}
