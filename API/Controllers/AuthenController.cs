using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using API.Models;
using API.Models.Context;
using API.Utils;

using NET_base.Models;
using NET_base.Models.Common;
using AutoMapper;
using API.Models.DTO;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _secret;
        private readonly int _expirationHours;
        private readonly IMapper _mapper;

        public AuthenController(IConfiguration configuration, DBContext context, IMapper mapper)
        {
            _configuration = configuration;
            _secret = configuration["JwtSettings:Secret"];
            _expirationHours = int.Parse(configuration["JwtSettings:ExpirationHours"]);
            _context = context;
            _mapper = mapper;
        }

        // Authentication - Login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<Response<string>> Authen(AuthenDTO dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null || !VerifyPassword(user.Password, dto.Password))
                {
                    return new Response<string>(false, "Invalid username or password", null);
                }

                if (user.IsDeleted == true)
                {
                    return new Response<string>(false, "You has been banned!", null);
                }

                if (user.Token != null)
                {
                    return new Response<string>(false, "Please verify your email address first.", null);
                }

                var token = GenerateJwtToken(user);

                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new Response<string>(true, Constant.SUCCESS_MESSAGE, token);
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return new Response<string>(false, Constant.FAIL_MESSAGE, null);
            }
        }

        // Registration - Register new user
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<Response<bool>> Register(AuthenDTO dto)
        {
            try
            {

                if (string.IsNullOrEmpty(dto.Email) || !dto.Email.Contains("@"))
                    return new Response<bool>(false, "Invalid email format.", false);

                // Check if the user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (existingUser != null)
                {
                    if (existingUser.Token == null)
                        return new Response<bool>(false, "User already exists with the given email", false);
                    else
                        return new Response<bool>(true, "User already registered, just verify email.", true);
                }

                string token = new Random().Next(100000, 1000000).ToString();

                // Create a new user
                var newUser = new User
                {
                    Username = dto.Email.Split("@").First(),
                    Email = dto.Email,
                    Password = HashPassword(dto.Password),
                    FullName = dto.Email.Split("@").First(),
                    Role = Constant.USER_ROLE,
                    IsDeleted = false,
                    Token = token,
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                EmailService.SendMailAsync(
                    dto.Email,
                    "Verify Your Email Address",
                    $@"
                    Dear {newUser.Username},

                    Thank you for registering with our service. To complete your registration, please verify your email address.

                    Your verification code is: {token}

                    Please enter this code on the verification page to activate your account. If you did not register for this account, please ignore this email.

                    Regards,  
                    Charity Team");


                return new Response<bool>(true, "User registered successfully.", true);
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return new Response<bool>(false, "An error occurred during registration", false);
            }
        }

        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<Response<bool>> VerifyEmail(VerifyDTO dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => (u.Email == dto.Email || u.Username == dto.Email) && u.Token == dto.Token);

                if (user == null)
                {
                    return new Response<bool>(false, "Invalid token or email.", false);
                }

                user.Token = null;
                await _context.SaveChangesAsync();

                return new Response<bool>(true, "Email verified successfully.", true);
            }
            catch (Exception ex)
            {
                return new Response<bool>(false, "An error occurred during email verification", false);
            }
        }

        // Helper function to generate JWT Token
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id.ToString(), ClaimValueTypes.Integer),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString(), ClaimValueTypes.Integer),
                    new Claim("isAdmin", (user.Role == Constant.ADMIN_ROLE).ToString(), ClaimValueTypes.Boolean)
                }),
                Expires = DateTime.UtcNow.AddHours(_expirationHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string HashPassword(string password)
        {
            var passwordHasher = new PasswordHasher<object>();
            return passwordHasher.HashPassword(null, password);
        }

        private bool VerifyPassword(string hashedPassword, string password)
        {
            var passwordHasher = new PasswordHasher<object>();
            var result = passwordHasher.VerifyHashedPassword(null, hashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
