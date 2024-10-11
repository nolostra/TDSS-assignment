namespace LinenManagementSystem.DTOs.Auth
{
    public class LoginRequestDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }


    public class LogoutRequest
    {
        public required string RefreshToken { get; set; }
    }

    public class RefreshRequest
    {
        public required string RefreshToken { get; set; }
    }

      public class AuthResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}