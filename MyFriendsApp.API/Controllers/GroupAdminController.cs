using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyFriendsApp.API.Data;
using MyFriendsApp.API.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MyFriendsApp.API.DTOs;
using System.Security.Claims;

namespace MyFriendsApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupAdminController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<User> _userManager;
        public RoleManager<Role> _roleManager { get; set; }

        private readonly IGroupRepository _groupRepo;
        private readonly SignInManager<User> _signInManager;

        public GroupAdminController(IGroupRepository groupRepo, UserManager<User> userManager,
                 RoleManager<Role> roleManager, SignInManager<User> signInManager, DataContext dataContext)
        {
            _signInManager = signInManager;
            _groupRepo = groupRepo;
            _roleManager = roleManager;
            _userManager = userManager;
            _dataContext = dataContext;
        }

        // [HttpPost("createGroup/{name}")]
        // public async Task<IActionResult> CreateGroup(string name)
        // {
        //     var grp = await _groupRepo.CreateGroup(name);
        //     if (grp != null)
        //     {
        //         return Ok("Group created");
        //     }
        //     return BadRequest("Failed to create group");
        // }

        [HttpGet("getUserGroups/{id}")]
        public async Task<IEnumerable<Group>> GetUserGroupsAsync(int id)
        {
            //var result = new List<Group>();
            //var result = await _dataContext.Groups.FirstOrDefaultAsync();
            var userGroups = await _groupRepo.GetUserGroupsAsync(id);
            return userGroups;
        }

        [HttpGet("getGroupRoles/{groupId}")]
        public async Task<IEnumerable<string>> GetGroupRolesAsync(
            int groupId)
        {
            var grp = await _dataContext.Groups.Include(k => k.GroupRoles)
                .FirstOrDefaultAsync(g => g.Id == groupId);
            var roles = await _roleManager.Roles.ToListAsync();
            var groupRoles = (from r in roles
                              where grp.GroupRoles
                                .Any(ap => ap.RoleId == r.Id)
                              select r.Name).ToList();
            return groupRoles;
        }
        [HttpGet("getGroupUsers/{groupId}")]
        public async Task<IEnumerable<string>> GetGroupUsersAsync(int groupId)
        {
            var usr = await _dataContext.Groups.Include(k => k.GroupUsers)
                .FirstOrDefaultAsync(g => g.Id == groupId);
            var users = await _userManager.Users.ToListAsync();
            var groupUsers = (from r in users
                              where usr.GroupUsers
                                .Any(ap => ap.UserId == r.Id)
                              select r.KnownAs).ToList();
            return groupUsers;
        }
        [HttpGet("getGroupUsersByGroupName/{groupName}")]
        public async Task<IEnumerable<string>> GetGroupUsersAsync(string groupName)
        {

            var users = await _userManager.Users.ToListAsync();
            
            
             var usr = await  _dataContext.Groups.Include(k => k.GroupUsers)
                 .FirstOrDefaultAsync(g => g.Name == groupName);
            
            var groupUsers = (from r in users
                              where usr.GroupUsers
                                .Any(ap => ap.UserId == r.Id)
                              select r.KnownAs).ToList();
            return groupUsers;
            
        }

        [HttpGet("getUserGroupRoles/{userId}/{groupId}")]
        public async Task<Group> GetUserGroupRoles(int userId, int groupId)
        {
            //var userGroups = await this.GetUserGroupsAsync(userId);
            var grp = await _dataContext.Groups
                    .Include(k => k.GroupRoles).ThenInclude(kk => kk.Role)
                    .Include(k => k.GroupUsers).ThenInclude(qq => qq.User)
                    .Where(k => k.GroupUsers.Any(ss => ss.UserId == userId))
                    .FirstOrDefaultAsync(g => g.Id == groupId);


            return grp;
        }

        [HttpGet("checkUserInGroup/{id}/{groupName}")]
        public async Task<IActionResult> CheckUserInGroup(int id, string groupName)
        {
            
           bool  isCurrentlUserAMemberAlready = await _groupRepo.CheckUserInGroup(id, groupName);
            

            if(isCurrentlUserAMemberAlready)
                return Ok("User in group!");

            return BadRequest("User not in group");

        }
        [HttpGet("checkUserRoleInGroup/{id}/{groupName}")]
        public async Task<IActionResult> CheckUserRoleInGroup(int id, string groupName)
        {
            
            int res = await _groupRepo.CheckUserRoleInGroup(id, groupName);
            return Ok(res);


        }

        [HttpGet("getUserMessagesFromGroup/{id}/{groupName}")]
        public async Task<IActionResult> GetUserMessagesFromGroup(int id, string groupName)
        {

            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (id != loggedInUserId)
                return Unauthorized();

            bool  isCurrentlUserAMemberAlready = await _groupRepo.CheckUserInGroup(id, groupName);
            if(!isCurrentlUserAMemberAlready)
                return BadRequest("User not in group");
            
            var messages = await _groupRepo.GetUserMessagesInGroup(id, groupName);
            if(messages != null)
                return Ok(messages);

            return BadRequest("Could not find messages");

        }



        [HttpPost("createGroupWithUsers")]
        public async Task<IActionResult> CreateGroupWithUsers([FromBody]GroupCreateDto groupCreateDto)
        {
            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (groupCreateDto.UserId != loggedInUserId)
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(groupCreateDto.UserId.ToString());
            if (user == null)
                return BadRequest("User doesnt exist");

            
            
            // check if currently LoggedIn User already a member of the group

            bool isCurrentlyLoggedinUserAMemberAlready = await _groupRepo.CheckUserInGroup(user.Id, groupCreateDto.GroupName);


            //check here if mentioned group has already in userHasGroups
            // if so do everything other than creating group.

            if (!isCurrentlyLoggedinUserAMemberAlready)
            {
                // first step create group
                var group = await _groupRepo.CreateUserGroupnUserRole(user, groupCreateDto.GroupName, true);
                if(!group)
                    return BadRequest("Failed to create group");
                // then create UserGroup
            }

            // add group members

            foreach (var id in groupCreateDto.GroupMembers)
            {
                var userMember = await _userManager.Users
                                .FirstOrDefaultAsync(kk => kk.Id == int.Parse(id));

                bool isMemberAlready = await _groupRepo.CheckUserInGroup(userMember.Id, groupCreateDto.GroupName);
                if(!isMemberAlready)
                {
                   var group = await _groupRepo.CreateUserGroupnUserRole(userMember, groupCreateDto.GroupName, false);
                   if(!group)
                    return BadRequest("Failed to create members in group");
                }
            }

            return Ok("Created all successfully");
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("addUserToGroup/{userName}/{groupName}")]
        public async Task<IActionResult> AddUserToGroup(string userName, string groupName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return BadRequest("User doesnt exist");

            var group = await _dataContext.Groups.FirstOrDefaultAsync(k => k.Name == groupName);
            if (group == null)
                return BadRequest("Group doesnt exist");


            var checkIfUserGroupAlreadyExist = _dataContext.UserGroups.Any(k => k.Group.Name == groupName && k.UserId == user.Id);
            if (checkIfUserGroupAlreadyExist)
                return BadRequest("User already exist");

            //user.Groups.Add(group);

            bool IshallBeGpAdmin = false;
            int roleId;
            Role role = new Role();
            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (loggedInUserId == user.Id)
            {
                IshallBeGpAdmin = true;
                role = await _roleManager.FindByNameAsync("_Gp_Admin");
                roleId = role.Id;
            }
            else
            {
                role = await _roleManager.FindByNameAsync("_Gp_Member");
                roleId = role.Id;
            }
            var currentUserRoles = await _userManager.GetRolesAsync(user);

            if (!currentUserRoles.Contains(role.Name))
            {
                IList<string> newrole = new List<string>();
                newrole.Add(role.Name);
                var result = await _userManager.AddToRolesAsync(user, newrole);
            }


            var usrGroup = new UserGroup();
            usrGroup.UserId = user.Id;
            usrGroup.GroupId = group.Id;
            usrGroup.Group = group;
            RoleGroup roleGroup = new RoleGroup();
            roleGroup.RoleId = roleId;
            roleGroup.Role = role;
            roleGroup.Group = group;
            roleGroup.GroupId = group.Id;

            _dataContext.UserGroups.AddAsync(usrGroup).Wait();
            _dataContext.RoleGroups.AddAsync(roleGroup).Wait();
            _dataContext.Groups.Update(group);

            if (await _dataContext.SaveChangesAsync() > 0)
            {
                return Ok(group);
            }
            return BadRequest("Failed to Save");


        }

        


        [HttpPost("createGroupMessage/{userId}/{groupName}/{message}")]
        public async Task<IActionResult> CreateGroupMessage(int userId, string groupName, string message)
        {
            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (userId != loggedInUserId)
                return Unauthorized();
            var user = await _userManager.Users
                                .Include(k => k.UserGroups).ThenInclude(kk => kk.Group).ThenInclude(kk => kk.GroupMessages)
                                .FirstOrDefaultAsync(qq => qq.Id == userId);
            if (user == null)
                return BadRequest("User doesnt exist");

            // check if currently LoggedIn User already a member of the group

            bool isCurrentlyLoggedinUserAMemberAlready = await _groupRepo.CheckUserInGroup(user.Id, groupName);
            if (!isCurrentlyLoggedinUserAMemberAlready)
                return BadRequest("User not in group");

            //get group

            var userGroups = user.UserGroups;

            var grp = userGroups.
                        FirstOrDefault(k => k.Group.Name == groupName).Group;

            // var grp = (from n in grp
            //             Include(kk => kk.Group).ThenInclude(qq => qq.GroupMessages))
            //             select n.Group;

            var mesge = new Message();
            mesge.Sender = user;
            mesge.Content = message;
            mesge.GroupMessage = grp;
            mesge.Recipient = user;

            _dataContext.Messages.Add(mesge); // comeback here nad comment
            _dataContext.SaveChanges();         // come back here and comment and see
            grp.GroupMessages.Add(mesge);


            var succeed = await _userManager.UpdateAsync(user);

            if(succeed.Succeeded)
                return Ok("Sent message successfully");

            return BadRequest("Failed to send message");

        }



    }
}