namespace UIAutomation.API
{
    public class AuthenticationResult
    {
        public bool Succeeded { get; set; } = true;
        public string Token { get; set; } = "";
        public string ErrorText { get; set; } = "";
    }
}