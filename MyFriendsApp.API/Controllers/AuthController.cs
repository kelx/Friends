using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyFriendsApp.API.Data;
using MyFriendsApp.API.DTOs;
using MyFriendsApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MyFriendsApp.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly DataContext _context;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, 
            DataContext context, IConfiguration config, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _config = config;
            _mapper = mapper;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            var usertoCreate = _mapper.Map<User>(userForRegisterDto);

            var result = await _userManager.CreateAsync(usertoCreate, userForRegisterDto.Password);

            var userToReturn = _mapper.Map<UserForDetailedDto>(usertoCreate);    

            if(result.Succeeded)
            {
                return CreatedAtRoute("GetUser",
                    new { controller = "Users", id = usertoCreate.Id }, userToReturn);    
            }

            return BadRequest(result.Errors);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserforLoginDto userForLoginDto)
        {

            var user = await _userManager.FindByNameAsync(userForLoginDto.Username);

            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDto.Password, false);

            if(result.Succeeded)
            {
                var appUser = await _userManager.Users
                    .Include(k => k.Photos)
                    .Include(k => k.UserGroups)
                    .FirstOrDefaultAsync(p => p.NormalizedUserName == userForLoginDto.Username.ToUpper());
                
                var groups = _context.Groups;
                var userToReturn = _mapper.Map<UserForListDto>(appUser);
                List<string> groupList = new List<string>();
                groupList = (List<string>)userToReturn.MyGroups;
                if (groupList.Count > 0)
                {
                    foreach (var id in groups)
                    {
                        for (int i = 0; i < groupList.Count; i++)
                        {
                            if (groupList[i] == id.Id.ToString())
                            {
                                groupList[i] = id.Name;
                            }
                        }
                    }
                }
                userToReturn.MyGroups = groupList;
                return Ok(new
                {
                    token = GenerateJwtToken(appUser).Result,
                    user = userToReturn
                });

            }

            return Unauthorized();

        }
        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
                
            };

            var roles = await _userManager.GetRolesAsync(user);

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}