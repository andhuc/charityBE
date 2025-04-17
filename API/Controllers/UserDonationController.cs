using API.Models;
using API.Models.Context;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NET_base.Models.Common;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDonationController : ControllerBase
    {

        private readonly DBContext _context;
        private readonly IMapper _mapper;

        public UserDonationController(DBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<Response<List<UserDonation>>> GetUserDonations([FromQuery] int? userId = null, [FromQuery] int? donationId = null)
        {
            var query = _context.UserDonations
                .Include(ud => ud.User)
                .Include(ud => ud.Donation)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(ud => ud.UserId == userId.Value);

            if (donationId.HasValue)
                query = query.Where(ud => ud.DonationId == donationId.Value);

            var list = await query.ToListAsync();

            return new Response<List<UserDonation>>(true, "Fetched user donations", list);
        }

        [HttpPost]
        [Authorize]
        public async Task<Response<bool>> Create(UserDonationDTO userDonationDTO)
        {
            try
            {
                userDonationDTO.UserId = JwtMiddleware.GetUserId(HttpContext);
                UserDonation userDonation = _mapper.Map<UserDonation>(userDonationDTO);

                await _context.UserDonations.AddAsync(userDonation);
                await _context.SaveChangesAsync();
                return new Response<bool>(true, "User donation added", true);
            }
            catch (Exception ex)
            {
                return new Response<bool>(false, $"Error: {ex.Message}", false);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<Response<bool>> Delete(int id)
        {
            var record = await _context.UserDonations.FindAsync(id);
            if (record == null)
                return new Response<bool>(false, "User donation not found", false);

            try
            {
                _context.UserDonations.Remove(record);
                await _context.SaveChangesAsync();
                return new Response<bool>(true, "Deleted successfully", true);
            }
            catch (Exception ex)
            {
                return new Response<bool>(false, $"Error deleting: {ex.Message}", false);
            }
        }

    }
}
