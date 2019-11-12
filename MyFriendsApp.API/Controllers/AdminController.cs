using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFriendsApp.API.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyFriendsApp.API.DTOs;
using Microsoft.AspNetCore.Identity;
using MyFriendsApp.API.Models;

namespace MyFriendsApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<User> _userManager;

        public AdminController(DataContext dataContext, UserManager<User> userManager)
        {
            _dataContext = dataContext;
            _userManager = userManager;
        }

        [Authorize(Policy="RequireAdminRole")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            var userList = await ( from user in _dataContext.Users
            orderby user.UserName
            select new {
                Id = user.Id,
                UserName = user.UserName,
                Roles = (from usrRole in user.UserRoles
                          join role in _dataContext.Roles
                          on usrRole.RoleId equals role.Id
                          select role.Name).ToList()}    
            ).ToListAsync();

            return Ok(userList);
        }

        [Authorize(Policy="ModeratePhotoRole")]
        [HttpGet("photosForModeration")]
        public IActionResult GetPhotosForModeration()
        {
            return Ok("Only admins and moderators can see this");
        }

        [Authorize(Policy="RequireAdminRole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto)
        {
            var user = await _userManager.FindByNameAsync(userName);
            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = roleEditDto.RoleNames;

            // selectedRoles = selectedRoles != null ? selectedRoles : new string[]{};
            selectedRoles = selectedRoles ?? new string[] {};

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if(!result.Succeeded)
                return BadRequest("Failed to add to roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            
            if(!result.Succeeded)
                return BadRequest("Failed to remove roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }

    }
}