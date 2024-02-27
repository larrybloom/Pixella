using AutoMapper;
using Filmies_Data.Models;
using Filmzie.Context;
using Filmzie.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Filmzie.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private IConfiguration _configuration;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AuthController(UserManager<User> userManager, IConfiguration configuration, 
            SignInManager<User> signInManager, AppDbContext context, IMapper mapper)
        {
            _userManager = userManager;
            _configuration = configuration;
            _signInManager = signInManager;
            _context = context;
            _mapper = mapper;
        }



        /// <summary>
        /// Register as a new user. 
        /// </summary>
        /// <param name="regRequest">A DTO containing the user data.</param>
        /// <returns>A 201 - Created Status Code in case of success.</returns>
        /// <response code="201">User has been registered</response>                  
        /// <response code="403">User Already Exist</response>                
        /// <response code="500">Failed to create user!</response>  
        [HttpPost("signup")]
        public async Task<IActionResult> Register([FromBody] SignUpDto regRequest)
        {
            // Check if user exists
            var userExist = await _userManager.FindByEmailAsync(regRequest.Email);
            if (userExist != null)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new APIResponse { StatusCode = "Error", IsSuccess = false, Message = "User already exists!" });
            }

            User user = new User()
            {
                UserName = regRequest.userName,
                Email = regRequest.Email,
                FirstName = regRequest.FirstName,
                LastName = regRequest.LastName,
                PhoneNumber = regRequest.PhoneNumber,
                Password = regRequest.Password,
            };

            var result = await _userManager.CreateAsync(user, regRequest.Password);

            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status201Created,
                    new APIResponse { StatusCode = "Success", IsSuccess = true, Message = $"User created successfully." });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new APIResponse { StatusCode = "Error", IsSuccess = false, Message = "Failed to create user!" });
            }
        }



        /// <summary>
        /// Perform a user email login. 
        /// </summary>
        /// <param name="loginRequest">A DTO containing the user's credentials.</param>
        /// <returns>The Bearer Token (in JWT format).</returns>
        /// <response code="200">User has been logged in</response> 
        /// <response code="401">Login failed (unauthorized)</response>
        /// <response code="500">User does not exist (unauthorized)</response>
        [HttpPost("signin")]
        [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<IActionResult> Login(LoginRequestDTO loginRequest)
        {
            // Check if user exists
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
            if (user is null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                       new APIResponse { StatusCode = "Error", IsSuccess = false, Message = "User does not exist" });
            }

            if (user != null && await _userManager.CheckPasswordAsync(user, loginRequest.Password))
            {
                var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

                // Pass the user ID and authClaims to the GetToken method
                var JwtToken = GetToken(user.Id, authClaims);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(JwtToken),
                    expiration = JwtToken.ValidTo
                });
            }

            return StatusCode(StatusCodes.Status401Unauthorized,
                    new APIResponse { StatusCode = "False", IsSuccess = false, Message = "Wrong Email or paswword" });
        }


         /// <summary>
         /// Generate JWT Token
         /// </summary>
         /// <param name="userId"></param>
         /// <param name="authClaims"></param>
         /// <returns>JWT TOKEN</returns>
        private JwtSecurityToken GetToken(string userId, List<Claim> authClaims)
        {
            var authSignInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            // Add the user ID as a claim
            authClaims.Add(new Claim(ClaimTypes.NameIdentifier, userId));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(30),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSignInKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }



        /// <summary>
        ///  Change/Update password
        /// </summary>
        /// <param name="model"></param>
        /// <response code="200">Password changed successfully!</response> 
        /// <response code="400">Reset password failed</response> 
        [HttpPut("update-password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword([FromBody] PasswordUpdateModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Retrieve user from the database
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    // User not found
                    return NotFound(new { message = "User not found." });
                }

                // Validate the current password
                if (!await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    // Wrong password
                    return BadRequest(new { message = "Wrong password." });
                }

                // Update the password
                var result = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);

                if (!result.Succeeded)
                {
                    // Password update failed
                    return BadRequest(new { message = "Failed to update password." });
                }

                return Ok(new { message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }


        /// <summary>
        /// Get User Information
        /// </summary>
        /// <response code="200">User Information successfully retrieved!</response> 
        /// <response code="404">User Not found</response> 
        /// <response code="500">Unable to retrieve user information</response> 
        [HttpGet("info")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new APIResponse { StatusCode = "Error", IsSuccess = false, Message = "Unable to retrieve user information." });
                }

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound,
                        new APIResponse { StatusCode = "Error", IsSuccess = false, Message = "User not found!" });
                }

                var userDto = _mapper.Map<UserDTO>(user);

                return Ok(userDto);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new APIResponse { StatusCode = "Error", IsSuccess = false, Message = "An error occurred while retrieving user information." });
            }
        }


        /// <summary>
        /// Get User Favorites
        /// </summary>
        /// <response code="200">User Favorites successfully retrieved!</response>  
        /// <response code="500">Unable to retrieve user Favorites</response>
        [HttpGet("favorites")]
        [Authorize]
        public async Task<IActionResult> GetFavorites()
        {
            try
            {
                // Retrieve favorites for the authenticated user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var favorites = await _context.Favorites
                    .Where(f => f.UserId == userId)
                    .OrderByDescending(f => f.CreatedAt)
                    .ToListAsync();

                return Ok(favorites);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }


        /// <summary>
        ///  Remove a media from User Favorite list
        /// </summary>
        /// <param name="favoriteId"></param>
        /// <response code="200">Media successfully removed from Favorites!</response> 
        /// <response code="404">Media Not found</response> 
        /// <response code="500">Unable to retrieve Favorites information</response>
        [HttpDelete("favorites/{favoriteId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFavorite(string favoriteId)
        {
            try
            {
                if (favoriteId == null)
                {
                    return BadRequest(new { message = "Invalid favoriteId. It must be a positive integer." });
                }
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.MediaId == favoriteId && f.UserId == userId);

                if (favorite == null)
                {
                    return NotFound(new { message = "Favorite not found or doesn't belong to the authenticated user." });
                }
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Favorite removed successfully." });
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }


        /// <summary>
        ///  Add a media to Favorites List
        /// </summary>
        /// <param name="model"></param>
        /// <response code="201">Media successfully Added to Favorites!</response> 
        /// <response code="404">User Not found</response> 
        /// <response code="400">Bad Request!</response>
        /// <response code="409">Item is already a fovorite</response> 
        /// <response code="500">Internal server error</response>
        [HttpPost("addfavorites")]
        [Authorize]
        public async Task<IActionResult> AddFavorite([FromBody] FavoriteCreateModel model)
        {
            try
            {
                // Validate model and perform necessary checks
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Add logic to check if the item is already a favorite
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingfav = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId
                && f.MediaId == model.mediaId);

                if (existingfav != null)
                {
                    // Item is already a favorite, return a conflict response
                    return Conflict(new { message = "Item is already a favorite for the user." });
                }

                // Create and save the favorite
                var favorite = new Favorites
                {
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    MediaId = model.mediaId,
                    MediaTitle = model.mediaTitle,
                    MediaType = model.mediaType,
                    MediaPoster = model.mediaPoster,
                    MediaRate = model.mediaRate
                };

                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetFavorites), new { userId = favorite.UserId }, favorite);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }
    }
}
