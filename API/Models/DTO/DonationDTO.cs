namespace API.Models
{
    public partial class DonationDTO
    {
        public int? Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal GoalAmount { get; set; }
        public decimal RaisedAmount { get; set; } = 0;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
