using API.Models;
using API.Models.Context;
using API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NET_base.Models.Common;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DBContext _context;
        public readonly string SK = "habdjawhdbjwhavvvsdag";

        public PaymentController(IConfiguration configuration, DBContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost]
        [Route("pay")]
        [Authorize]
        public async Task<Response<string>> CreatePayment(OrderInfo order)
        {
            try
            {
                string vnp_Returnurl = "http://localhost:5001/api/Payment/notify?sk=" + SK;
                string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
                string vnp_TmnCode = "CGXZLS0Z";
                string vnp_HashSecret = "XNBCJFAKAZQSGTARRLGCHVZWCIOIGSHN";

                int? userId = JwtMiddleware.GetUserId(HttpContext);

                if (order == null || userId == null)
                    return new Response<string>(false, "Bad request.", null);

                string OrderId = DateTime.Now.Ticks.ToString();

                Transaction transaction = new Transaction();
                transaction.Amount = (decimal)order.Amount;
                transaction.UserId = (int)userId;
                transaction.OrderId = OrderId;

                await _context.Transactions.AddAsync(transaction);
                await _context.SaveChangesAsync();

                VnPayLibrary vnpay = new VnPayLibrary();
                vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                vnpay.AddRequestData("vnp_Amount", (order.Amount * 100).ToString()); // Convert to cents
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", VnPayLibrary.GetIpAddress(HttpContext));
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + OrderId);
                vnpay.AddRequestData("vnp_OrderType", "other");
                vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
                vnpay.AddRequestData("vnp_TxnRef", OrderId);

                string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

                return new Response<string>(true, "Success", paymentUrl);
            } catch (Exception ex)
            {
                return new Response<string>(true, ex.Message, null);
            }
        }

        [HttpGet]
        [Route("notify")]
        public async Task<IActionResult> PaymentNotify([FromQuery] string sk)
        {
            try
            {
                if (sk != SK)
                {
                    return Unauthorized();
                }

                // Check if query parameters are present
                if (Request.Query.Count == 0)
                {
                    return BadRequest(new Response<bool>(false, "Input data required", false));
                }

                string vnp_HashSecret = "XNBCJFAKAZQSGTARRLGCHVZWCIOIGSHN";
                var vnpayData = Request.Query;
                VnPayLibrary vnpay = new VnPayLibrary();

                // Add query parameters to VnPayLibrary
                foreach (var campain in vnpayData)
                {
                    if (!string.IsNullOrEmpty(campain.Value) && campain.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(campain.Key, campain.Value);
                    }
                }

                // Parse and validate amount
                if (!long.TryParse(vnpay.GetResponseData("vnp_Amount"), out long vnp_Amount))
                {
                    return BadRequest(new Response<bool>(false, "Invalid amount", false));
                }
                vnp_Amount /= 100;

                // Retrieve response data
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                string vnp_SecureHash = vnpay.GetResponseData("vnp_SecureHash");
                string vnp_TxnRef = vnpay.GetResponseData("vnp_TxnRef");

                // Validate secure hash
                if (string.IsNullOrEmpty(vnp_SecureHash))
                {
                    return BadRequest(new Response<bool>(false, "Invalid signature", false));
                }

                // Verify signature
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (!checkSignature)
                {
                    return BadRequest(new Response<bool>(false, "Invalid signature", false));
                }

                // Retrieve transaction from database
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.OrderId == vnp_TxnRef);

                if (transaction == null || transaction.Status != Constant.PENDING)
                {
                    return NotFound(new Response<bool>(false, "Invalid order or order already processed", false));
                }

                // Retrieve user associated with the transaction
                var user = await _context.Users
                    .FirstOrDefaultAsync(t => t.Id == transaction.UserId);

                if (user == null)
                {
                    return NotFound(new Response<bool>(false, "User not found", false));
                }

                // Update transaction and user credit
                if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                {
                    transaction.Status = Constant.SUCCESS;
                    await _context.SaveChangesAsync();

                    //EmailService.SendMailAsync(user.Email, "Recharge success", $"Recharge successful!\nHi {user.Username},\nYour new balance: {user.Credit / 1000} credits.\nThank you!");

                    return Redirect("http://localhost:5173/profile?successP");
                }
                else
                {
                    // Handle failed payment
                    transaction.Status = Constant.FAILED;
                    await _context.SaveChangesAsync();

                    return BadRequest(new Response<bool>(false, "Payment failed", false));
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, new Response<bool>(false, e.Message, false));
            }
        }
    }
}
