namespace ChkSDK.SettingsModel
{
    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public int ExpiryHours { get; set; }
    }
}
