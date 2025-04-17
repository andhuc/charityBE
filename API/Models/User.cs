using System;
using System.Collections.Generic;

namespace API.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string? Username { get; set; } = null!;
        public string? Email { get; set; } = null!;
        public string? Password { get; set; } = null!;
        public string? FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int? Role { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Token { get; set; }
        public DateTime? LastLogin { get; set; }

        public virtual Role? RoleNavigation { get; set; }
        public virtual ICollection<Notification>? Notifications { get; set; }
        public virtual ICollection<UserDonation>? UserDonations { get; set; }

    }
}
