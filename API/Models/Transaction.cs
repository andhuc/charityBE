using NET_base.Models.Common;

namespace API.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = Constant.PENDING;
        public DateTime CreatedAt { get; set; }

    }
}
