using API.Models;

namespace API.Models.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int Role { get; set; }
        public string? Token { get; set; }
        public string? RoleName { get; set; }
        public bool? IsDeleted { get; set; }

    }
}
