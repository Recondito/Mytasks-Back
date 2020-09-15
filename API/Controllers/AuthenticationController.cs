using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using API.DTOs;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IConfiguration config;
        private readonly AppIdentityContext appIdentityContext;
        private readonly SymmetricSecurityKey key;

        public AuthenticationController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager, IConfiguration config, AppIdentityContext appIdentityContext)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.config = config;
            this.appIdentityContext = appIdentityContext;
            this.key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]));
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await userManager.FindByNameAsync(loginDto.Username);

            if (user == null) return Unauthorized();

            var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if(!result.Succeeded) return Unauthorized();

            var tokenString = await GetToken(user);

            return Ok(new UserDto() { Email = user.Email, UserName = user.UserName, Token = tokenString });
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            using (var transaction = await appIdentityContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (CheckUserNameExists(registerDto.Username).Result.Value)
                    {
                        return StatusCode(StatusCodes.Status409Conflict, new Response { Status = "Conflict", Message = "User already exists" });
                    }

                    var user = new AppUser()
                    {
                        UserName = registerDto.Username,
                        Email = registerDto.Email
                    };

                    var result = await userManager.CreateAsync(user, registerDto.Password);
                    if (!result.Succeeded)
                    {
                        return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to create user" });
                    }

                    var userRole = "NormalUser";

                    if (!await roleManager.RoleExistsAsync(userRole))
                    {
                        await roleManager.CreateAsync(new IdentityRole(userRole));
                    }

                    var roleAddResult = await userManager.AddToRoleAsync(user, userRole);

                    if (!roleAddResult.Succeeded) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to add user role" });

                    user = await userManager.FindByNameAsync(registerDto.Username); //Updates user to include the userId in the token

                    if (user == null) return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Failed to find created user" });

                    var tokenString = await GetToken(user);

                    await transaction.CommitAsync();

                    return Ok(new UserDto() { Email = user.Email, UserName = user.UserName, Token = tokenString });
                }
                catch
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Failed to complete transaction" });
                }
                
            }
                       
        }

        [HttpGet("userexists")]
        public async Task<ActionResult<bool>> CheckUserNameExists([FromQuery] string username)
        {
            if(string.IsNullOrEmpty(username)) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Null or empty username" });

            return await userManager.FindByNameAsync(username) != null;            
        }

        [Authorize]
        [HttpPut("changepassword")]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var userId = HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "userid")?.Value;
            
            if (userId == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Invalid token" });

            var user = await userManager.FindByIdAsync(userId);

            if(user == null) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "User does not exist" });

            var result = await userManager.ChangePasswordAsync(user, changePasswordDto.currentPassword, changePasswordDto.newPassword);

            if(!result.Succeeded) return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "Failed to change password" });

            return Ok(new Response() { Status = "Success", Message = "Password was changed successfully" });
        }


        private async Task<string> GetToken(AppUser user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("userid", user.Id)
            };
            var roles = await userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: config["JWT:Issuer"],
                expires: DateTime.Now.AddHours(3),
                claims: claims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return  new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
