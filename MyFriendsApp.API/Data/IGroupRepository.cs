using System.Collections.Generic;
using System.Threading.Tasks;
using MyFriendsApp.API.Models;

namespace MyFriendsApp.API.Data
{
    public interface IGroupRepository
    {

         void Add<T>(T entity) where T: class;
         void Delete<T>(T entity) where T: class;
         void Update<T>(T entity) where T: class;
        Task<bool> SaveAll();
        Task<Group> GetGroup(int groupId);
        int GetGroupId(string groupName);
        Task<bool> CheckUserInGroup(int id, string groupName);
        Task<int> CheckUserRoleInGroup(int id, string groupName);
        Task<bool> CreateUserGroupnUserRole(User user, string groupName, bool iShallbeAdmin);
        Task<IEnumerable<Group>> GetUserGroupsAsync(int id);
        Task<Group> AddUsersToGroup(User user, Group group, ICollection<string> groupMembers);
        Task<IEnumerable<string>> GetUserMessagesInGroup(int id, string groupName);
    }
}