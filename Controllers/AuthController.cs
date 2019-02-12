using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        /// <summary>
        /// construtor with DJ, DI configured in startup.cs
        /// </summary>
        /// <param name="authRepository"></param>
        public AuthController(IAuthRepository authRepository, IConfiguration config)
        {
            this._repo = authRepository;
            this._config = config;
        }
        /// <summary>
        /// Register
        /// </summary>
        /// <param name="userForRegister"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserForRegisterDto userForRegister)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            userForRegister.Username = userForRegister.Username.ToLower();

            if (await _repo.UserExists(userForRegister.Username)) { return BadRequest("Username already exists"); }

            var userToCreate = new User { Username = userForRegister.Username };

            var createdUser = await _repo.Register(userToCreate, userForRegister.Password);

            return StatusCode(201); // means request successful and led to the creation of some resources
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="userForLoginDto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
                //throw new Exception("Computer says no");
                var userFromRepo = await this._repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
                if (userFromRepo == null) { return Unauthorized(); }

                //return a toke, add JWTs token
                var claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name,userFromRepo.Username)
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._config.GetSection("AppSettings:Token").Value));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.Now.AddDays(1),
                    SigningCredentials = creds
                };

                var tokenHandler = new JwtSecurityTokenHandler();

                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

                return Ok(new
                {
                    Token = tokenHandler.WriteToken(token)
                }); //anonymous type as an object
        }
    }
}