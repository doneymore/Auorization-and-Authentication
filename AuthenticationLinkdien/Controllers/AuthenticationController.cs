using AuthenticationLinkdien.Dtos;
using AuthenticationLinkdien.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationLinkdien.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _user;
        private readonly RoleManager<IdentityRole> _role;
        private readonly AppDbConfig _db;
        private readonly IConfiguration _config;
        private readonly TokenValidationParameters _refresh;

        public AuthenticationController(UserManager<ApplicationUser> user, RoleManager<IdentityRole> role, AppDbConfig db, IConfiguration  configuration, TokenValidationParameters refresh)
        {
            this._user = user;
            this._role = role;
            this._db = db;
            this._config = configuration;
            this._refresh = refresh;
        }
        [HttpPost("Register-User")]
        public async Task<IActionResult> Register([FromBody] RegisterDto reg)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please Provide All The Required Fields");
            }

            var userExists = await _user.FindByEmailAsync(reg.Email);

             if (userExists != null)
            {
                return BadRequest($"User {reg.Email} Already Exist");
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                FirstName = reg.FirstName,
                LastName = reg.LastName,
                Email = reg.Email,
                UserName = reg.UserName,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _user.CreateAsync(newUser , reg.Password);

            if (result.Succeeded)
            {
                switch (reg.Role)
                {
                    case SeedFile.Manager:
                        await _user.AddToRoleAsync(newUser, SeedFile.Manager);
                        break;

                    case SeedFile.Student:
                        await _user.AddToRoleAsync(newUser, SeedFile.Student);
                        break;
                    default:
                        break;
                }
            }
                return Ok("User Crreated");
            return BadRequest("User Could not be Created");


        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto logs)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, Provide all required field");
            }

            var userExist = await _user.FindByEmailAsync(logs.Email);
            var checkPassword = await _user.CheckPasswordAsync(userExist, logs.Password);
            if(userExist != null && checkPassword)
            {
                var tokenValue = await GenerateToken(userExist, null);
                return Ok(tokenValue);
            }
            return Unauthorized();
        }



        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRefresh toks)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please, Provide all required field");
            }

            var result = await VerifyandRefeshfTokenAsync(toks);
            return Ok(result);  
          
        }


        private async Task<AuthId> VerifyandRefeshfTokenAsync(TokenRefresh toks)
        {
           var jwtTokenHandler = new JwtSecurityTokenHandler();
           var storedToken = await _db.RefToken.FirstOrDefaultAsync(x => x.Token == toks.RefreshTok);
            var dbUser = await _user.FindByIdAsync(storedToken.UserId);

            try
            {
                var tokenCheckResult = jwtTokenHandler.ValidateToken(toks.Token, _refresh, out var validateToken);

                return await GenerateToken(dbUser, storedToken);
            }
            catch (SecurityTokenExpiredException)
            {

                if(storedToken.DateExpired >= DateTime.UtcNow)
                {
                       return  await GenerateToken(dbUser, storedToken);
                }else
                {
                       return   await GenerateToken(dbUser, null);
                }
            }
        }


        private async Task<AuthId> GenerateToken(ApplicationUser user, RefreshToken rToken )
        {
            //Generate User Claims
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //new Claim(JwtRegisteredClaimNames.Aud, _config["Jwt:Audience"]),
                new Claim(JwtRegisteredClaimNames.Iss, _config["Jwt:Issuer"])
            };

            //Generate Roles Claims
            var userRoles = await _user.GetRolesAsync(user);
            foreach (var item in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, item));
            }


            //Bring in the SecrectKey or Key Saved on the startup.cs
            var authSigninKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Jwt:Secret"]));


            //Geneake the Token payload
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.UtcNow.AddMinutes(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigninKey, SecurityAlgorithms.HmacSha256)

             );

            //GenerateToken 
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            if(rToken != null)
            {
             var rTokenRespo = new AuthId
                {
                    Token = jwtToken,
                    RefreshToken = rToken.Token,
                    ExpiresAt = token.ValidTo
                };
                return rTokenRespo;
            }

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsRevoked = false,
                UserId = user.Id,
                DateAdded = DateTime.UtcNow,
                DateExpired = DateTime.UtcNow.AddMonths(6),
                Token = Guid.NewGuid().ToString() +  "_" + Guid.NewGuid().ToString()
            };
            await _db.RefToken.AddAsync(refreshToken);
            await _db.SaveChangesAsync();

            var response = new AuthId
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = token.ValidTo
            };
            return response;

        }
    }
}
