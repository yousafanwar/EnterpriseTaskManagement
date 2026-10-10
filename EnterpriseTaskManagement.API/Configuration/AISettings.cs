namespace EnterpriseTaskManagement.API.Configuration
{
    public class AISettings
    {
        public string Provider { get; set; } = "Gemini";
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }
}