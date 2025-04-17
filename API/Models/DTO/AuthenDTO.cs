namespace API.Models.DTO
{
    public class AuthenDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class VerifyDTO
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
