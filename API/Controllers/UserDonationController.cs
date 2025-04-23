using API.Models;
using API.Models.Context;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NET_base.Models.Common;
using System.Linq.Dynamic.Core;

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
        public async Task<Response<PagedResult<UserDonationDTO>>> GetUserDonations(
                [FromQuery] int? donationId = null,
                [FromQuery] int? userId = null,
                [FromQuery] string? search = null,
                [FromQuery] int page = 1,
                [FromQuery] int size = 10)
        {
            var query = _context.UserDonations
                .Include(ud => ud.User)
                .Include(ud => ud.Donation)
                .AsNoTracking()
                .AsQueryable();

            if (donationId.HasValue)
                query = query.Where(ud => ud.DonationId == donationId.Value);

            if (userId.HasValue)
                query = query.Where(ud => ud.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalized = search.Trim().ToLower();
                query = query.Where(ud =>
                    ud.User.FullName.ToLower().Contains(normalized) ||
                    ud.User.Email.ToLower().Contains(normalized) ||
                    ud.Donation.Title.ToLower().Contains(normalized));
            }

            var totalCount = await query.CountAsync();

            var userDonations = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var userDonationDTOs = _mapper.Map<List<UserDonationDTO>>(userDonations);

            return new Response<PagedResult<UserDonationDTO>>(
                true,
                "Fetched user donations",
                new PagedResult<UserDonationDTO>
                {
                    Queryable = userDonationDTOs.AsQueryable(),
                    RowCount = totalCount,
                    CurrentPage = page,
                    PageSize = size,
                    PageCount = (int)Math.Ceiling((double)totalCount / size)
                });
        }


        [HttpPost]
        [Authorize]
        public async Task<Response<bool>> Create(UserDonationDTO userDonationDTO)
        {
            try
            {
                bool donationExists = await _context.Donations.AnyAsync(d => d.Id == userDonationDTO.DonationId);
                if (!donationExists)
                {
                    return new Response<bool>(false, "Donation not found.", false);
                }

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
