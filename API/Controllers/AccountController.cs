using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper) : APIControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if (await UserExists(registerDTO.Username))
            {
                return BadRequest("Username is taken");
            }

            var user = mapper.Map<AppUser>(registerDTO);
            user.UserName = registerDTO.Username.ToLower();

            var result = await userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new UserDTO
            {
                Username = user.UserName,
                Token = await tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender,
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            AppUser? user = await userManager.Users.Include(x => x.Photos).FirstOrDefaultAsync(x => x.NormalizedUserName == loginDTO.Username.ToUpper());

            if (user == null || user.UserName == null)
            {
                return Unauthorized("Invalid Username");
            }

            var result = await userManager.CheckPasswordAsync(user, loginDTO.Password);

            if (!result)
            {
                return Unauthorized();
            }

            return Ok(new UserDTO
            {
                Username = user.UserName,
                Token = await tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            });
        }

        private async Task<bool> UserExists(string username)
        {
            return await userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
        }
    }
}
