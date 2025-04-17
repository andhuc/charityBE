public class UserDonationDTO
{
    public int? Id { get; set; }
    public int? UserId { get; set; }
    public int DonationId { get; set; }
    public decimal Amount { get; set; }

    public string? UserFullname { get; set; }
    public string? DonationTitle { get; set; }
}
