using System;

namespace API.Models
{
    public partial class UserDonation
    {
        public int UserId { get; set; }
        public int DonationId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DonationDate { get; set; } = DateTime.Now;

        public virtual User User { get; set; } = null!;
        public virtual Donation Donation { get; set; } = null!;
    }
}
