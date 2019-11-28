using System.Collections.Generic;
using System.Threading.Tasks;
using MyFriendsApp.API.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Diagnostics;

namespace MyFriendsApp.API.Data
{
    public class GroupRepository : IGroupRepository
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        public GroupRepository(DataContext data_Context, UserManager<User> userManager,
                                RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _dataContext = data_Context;

        }
    public void Update<T>(T entity) where T : class
    {
        _dataContext.Update(entity);
    }
    public void Add<T>(T entity) where T : class
    {
        _dataContext.Add(entity);
    }

    public void Delete<T>(T entity) where T : class
    {
        _dataContext.Remove(entity);
    }

    public Task<Group> GetGroup(int groupId)
    {
        throw new System.NotImplementedException();
    }
    

    public Task<bool> SaveAll()
    {
        throw new System.NotImplementedException();
    }

    public async Task<Group> CreateUserGroupnUserRole(User user, string groupName, bool IshallBeGpAdmin)
    {
        Group grp = new Group();
        grp.Name = groupName;

        //check if group already exists
        bool grpAlreadyExist = _dataContext.Groups.Any(kk =>kk.Name == groupName);

        if(grpAlreadyExist)
        {
            grp = await _dataContext.Groups.FirstOrDefaultAsync(kk => kk.Name == groupName);
            //return grp;
        }
        // add user to groups
        // create User Group and then add to group

        UserGroup userGroup = new UserGroup();
        userGroup.User = user;
        userGroup.UserId = user.Id;
        userGroup.GroupId =  grp.Id;
        userGroup.Group = grp;

        RoleGroup roleGroup = new RoleGroup();
        roleGroup.UserId = user.Id;
        roleGroup.GroupId = grp.Id;
        roleGroup.Group = grp;

        
        int roleId;
        Role role = new Role();
        // create role _Gp_Admin
        
        if(IshallBeGpAdmin) 
        {
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

        //add role to rolegroup

        roleGroup.Role = role;
        roleGroup.RoleId = roleId;
        _dataContext.UserGroups.AddAsync(userGroup).Wait();
        _dataContext.RoleGroups.AddAsync(roleGroup).Wait();
        
        grp.GroupRoles.Add(roleGroup);
        grp.GroupUsers.Add(userGroup);
        
            // if group already exist then it has to be updated
        if (!grpAlreadyExist)
            _dataContext.Groups.AddAsync(grp).Wait();
        else
            _dataContext.Groups.Update(grp);

        await _dataContext.SaveChangesAsync();
        user.UserGroups.Add(userGroup);
            
        user.Groups.Add(grp);
            
        
        _userManager.UpdateAsync(user).Wait();
        
        
        return grp;
    }

    public async Task<IEnumerable<Group>> GetUserGroupsAsync(int id)
    {
        var userGroups = (from g in _dataContext.Groups
                          where g.GroupUsers
                            .Any(u => u.UserId == id)
                          select g).ToListAsync();

        return await userGroups;
    }

    public async Task<Group> AddUsersToGroup(User user, Group group, ICollection<string> groupMembers)
    {
        bool IshallBeGpAdmin;

        //check if user already in group

        var alreadyMember =  (from g in _dataContext.Groups
                            .Include(k => k.GroupUsers)
                          where g.GroupUsers
                            .Any(u => u.UserId == user.Id)
                          select(g.Id));
                          

        //_dataContext.Groups.Include(kk => kk.GroupUsers).Where(kk => kk.GroupUsers.FirstOrDefault(qq => qq.UserId==user.Id));
        // Add UserAdmin to Group
        //Debug.WriteLine(alreadyMember.ToString());
        if(alreadyMember == null)
             group = await AddUserToGroup(user, group, IshallBeGpAdmin = true);
            
        foreach (var id in groupMembers)
        {
            var userMember = await _userManager.FindByIdAsync(id);
            group = await AddUserToGroup(userMember, group, IshallBeGpAdmin = false);
        }
        return group;
        
    }

        private async Task<Group> AddUserToGroup(User user, Group group, bool IshallBeGpAdmin)
        {
        int roleId;
        Role role = new Role();
        // create role _Gp_Admin
        
        if(IshallBeGpAdmin) 
        {
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
        roleGroup.UserId = user.Id;
        roleGroup.Role = role;
        roleGroup.Group = group;
        roleGroup.GroupId = group.Id;

        _dataContext.UserGroups.AddAsync(usrGroup).Wait();
        _dataContext.RoleGroups.AddAsync(roleGroup).Wait();
        _dataContext.Groups.Update(group);

        if (await _dataContext.SaveChangesAsync() > 0)
        {
            return group;
        }
        return null;
        }
    }
}