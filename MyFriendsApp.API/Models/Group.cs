using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
namespace MyFriendsApp.API.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl {get; set;}
        //public ICollection<Photo> GroupPhotos {get; set;}
        public ICollection<Message> GroupMessages {get; set;}
        public ICollection<RoleGroup> GroupRoles {get; set;}
        public ICollection<UserGroup> GroupUsers {get; set;}
    }
}