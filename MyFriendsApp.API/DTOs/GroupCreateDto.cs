using System.Collections.Generic;

namespace MyFriendsApp.API.DTOs
{
    public class GroupCreateDto
    {
        public int UserId { get; set; }
        public string GroupName { get; set; }
        public ICollection<string> GroupMembers { get; set; }
    
    }
}