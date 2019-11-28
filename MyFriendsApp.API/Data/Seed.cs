using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using MyFriendsApp.API.Models;
using Newtonsoft.Json;

namespace MyFriendsApp.API.Data
{
    public class Seed
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly DataContext _dbcontext;

        public Seed(UserManager<User> userManager, RoleManager<Role> roleManager, DataContext dbcontext)
        {
            _dbcontext = dbcontext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void SeedUsers()
        {
            if (!_userManager.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                var roles = new List<Role>
                {
                    new Role {Name = "Member"},
                    new Role {Name = "Admin"},
                    new Role {Name = "Moderator"},
                    new Role {Name = "VIP"},
                    new Role {Name = "_Gp_Member"},
                    new Role {Name = "_Gp_Admin"},
                    new Role {Name = "_Gp_Moderator"},
                    new Role {Name = "_Gp_VIP"}
                };
                foreach (var role in roles)
                {
                    _roleManager.CreateAsync(role).Wait();
                }

                var groups = new List<Group>
                {
                    new Group {Name = "Family"},
                    new Group {Name = "Friends"}
                };


                foreach (var user in users)
                {
                    _userManager.CreateAsync(user, "password").Wait();
                    _userManager.AddToRoleAsync(user, "Member");
                }

                var admin = _userManager.FindByNameAsync("kelmen").Result;
                _userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" }).Wait();
                
                foreach(var gr in groups)
                {
                   _dbcontext.Groups.Add(gr);
                }
                _dbcontext.SaveChanges();


            }
        }

    }
}