using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController(DataContext context, ITokenService tokenService, IMapper mapper) : APIControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            if (await UserExists(registerDTO.Username))
            {
                return BadRequest("Username is taken");
            }

            HMACSHA512 hmac = new();

            var user = mapper.Map<AppUser>(registerDTO);
            user.UserName = registerDTO.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
            user.PassswordSalt = hmac.Key;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return Ok(new UserDTO
            {
                Username = user.UserName,
                Token = tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender,
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            AppUser? user = await context.Users.Include(x => x.Photos).FirstOrDefaultAsync(x => x.UserName.ToLower() == loginDTO.Username.ToLower());

            if (user == null)
            {
                return Unauthorized("Invalid Username");
            }

            HMACSHA512 hmac = new()
            {
                Key = user.PassswordSalt
            };

            byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if(computedHash[i] != user.PasswordHash[i])
                {
                    return Unauthorized("Invalid Password");
                }
            }

            return Ok(new UserDTO
            {
                Username = user.UserName,
                Token = tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            });
        }

        private async Task<bool> UserExists(string username)
        {
            return await context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower());
        }
    }
}
