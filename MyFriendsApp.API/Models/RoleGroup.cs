namespace MyFriendsApp.API.Models
{
    public class RoleGroup
    {
        public int RoleId { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public Role Role { get; set; }
        public Group Group { get; set; }
        //public User User {get; set;}
    }
}