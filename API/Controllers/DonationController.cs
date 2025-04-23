using API.Models;
using API.Models.Context;
using API.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using NET_base.Models.Common;
using System.Linq.Dynamic.Core;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly IMapper _mapper;

        public DonationController(DBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<Response<PagedResult<DonationDTO>>> GetDonations(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] string? search = null)
        {
            var donationsQuery = _context.Donations.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalized = search.Trim().ToLower();
                donationsQuery = donationsQuery.Where(d =>
                    d.Title.ToLower().Contains(normalized) ||
                    d.Description.ToLower().Contains(normalized));
            }

            var totalDonations = await donationsQuery.CountAsync();

            var donations = await donationsQuery
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            var donationDTOs = _mapper.Map<List<DonationDTO>>(donations);

            return new Response<PagedResult<DonationDTO>>(
                true,
                "Donations fetched successfully.",
                new PagedResult<DonationDTO>
                {
                    Queryable = donationDTOs.AsQueryable(),
                    RowCount = totalDonations,
                    CurrentPage = page,
                    PageSize = size,
                    PageCount = (int)Math.Ceiling((double)totalDonations / size)
                });
        }

        [HttpGet("{id}")]
        public async Task<Response<DonationDTO>> GetDonationById(int id)
        {
            var donation = await _context.Donations.FindAsync(id);
            if (donation == null)
            {
                return new Response<DonationDTO>(false, "Donation not found", null);
            }

            var donationDTO = _mapper.Map<DonationDTO>(donation);
            return new Response<DonationDTO>(true, "Donation fetched successfully.", donationDTO);
        }

        [HttpPost]
        [Authorize("Admin")]
        public async Task<Response<bool>> AddDonation(DonationDTO newDonationDto)
        {
            try
            {
                Donation donation = _mapper.Map<Donation>(newDonationDto);
                await _context.Donations.AddAsync(donation);
                await _context.SaveChangesAsync();

                return new Response<bool>(true, "Donation created successfully.", true);
            }
            catch (Exception ex)
            {
                return new Response<bool>(false, $"Error creating donation: {ex.Message}", false);
            }
        }

        [HttpPut]
        [Authorize("Admin")]
        public async Task<Response<bool>> UpdateDonation(DonationDTO updatedDonation)
        {
            var donation = await _context.Donations.FindAsync(updatedDonation.Id);
            if (donation == null)
            {
                return new Response<bool>(false, "Donation not found", false);
            }

            _mapper.Map(updatedDonation, donation);

            try
            {
                _context.Donations.Update(donation);
                await _context.SaveChangesAsync();
                return new Response<bool>(true, "Donation updated successfully.", true);
            }
            catch (Exception ex)
            {
                return new Response<bool>(false, $"Error updating donation: {ex.Message}", false);
            }
        }

        [HttpDelete("{id}")]
        [Authorize("Admin")]
        public async Task<Response<bool>> DeleteDonation(int id)
        {
            var donation = await _context.Donations.FindAsync(id);
            if (donation == null)
            {
                return new Response<bool>(false, "Donation not found", false);
            }

            try
            {
                _context.Donations.Remove(donation);
                await _context.SaveChangesAsync();
                return new Response<bool>(true, "Donation deleted successfully.", true);
            }
            catch (Exception ex)
            {
                return new Response<bool>(false, $"Error deleting donation: {ex.Message}", false);
            }
        }
    }
}
