using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MyFriendsApp.API.Models
{
    public class Role : IdentityRole<int>
    {
        public ICollection<UserRole> UserRoles { get; set; }
    }
}