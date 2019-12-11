using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyFriendsApp.API.Helpers;
using MyFriendsApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MyFriendsApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        private readonly IGroupRepository _groupRepo;
        public DatingRepository(DataContext context, IGroupRepository groupRepo)
        {
            _groupRepo = groupRepo;
            this._context = context;

        }
        public void Update<T>(T entity) where T : class
        {
            _context.Update(entity);
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(k =>
                k.LikerId == userId && k.LikeeId == recipientId);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(kp => kp.UserId == userId)
                .FirstOrDefaultAsync(k => k.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(k => k.Id == id);
            return user;
        }
        public async Task<IEnumerable<User>> GetUsers(int userId)
        {
            var users = await _context.Users.Include(p => p.Photos)
                            .Where(kkk => kkk.Id != userId).ToListAsync();
            return users;
        }
        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(p => p.Photos)
            .OrderByDescending(k => k.LastActive).AsQueryable();

            users = users.Where(k => k.Id != userParams.UserId);

            users = users.Where(k => k.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));
            }
            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));

            }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDOB = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(k => k.DateOfBirth >= minDOB && k.DateOfBirth <= maxDob);
            }
            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(k => k.Created);
                        break;
                    default:
                        users = users.OrderByDescending(k => k.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var users = await _context.Users
                .Include(x => x.Likers)
                .Include(x => x.Likees)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (likers)
            {
                return users.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                return users.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }

        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                .Include(k => k.Sender).ThenInclude(q => q.Photos)
                .Include(k => k.Recipient).ThenInclude(q => q.Photos)
                .AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(k => k.RecipientId == messageParams.UserId && k.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(k => k.SenderId == messageParams.UserId && k.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(k => k.RecipientId == messageParams.UserId && k.RecipientDeleted == false
                         && k.IsRead == false);
                    break;
            }
            messages = messages.OrderByDescending(k => k.MessageSent);

            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessagesThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
                .Include(k => k.Sender).ThenInclude(q => q.Photos)
                .Include(k => k.Recipient).ThenInclude(q => q.Photos)
                .Where(k => k.RecipientId == userId && k.RecipientDeleted == false && k.SenderId == recipientId ||
                    k.RecipientId == recipientId && k.SenderId == userId && k.SenderDeleted == false)
                .OrderByDescending(k => k.MessageSent)
                .ToListAsync();

            return messages;
        }

        

        public async Task<User> GetUserWithGroup(int userId, string groupName)
        {
            
            int groupId = _groupRepo.GetGroupId(groupName);

            var user = await _context.Users.
                            Include(p => p.Photos).
                            Include(q => q.UserGroups).ThenInclude(q => q.Group).
                            Where(l => l.UserGroups.Any(m => m.GroupId == groupId)).
                            FirstOrDefaultAsync(k => k.Id == userId);
            //var groups = await _groupRepo.GetUserGroupsAsync(userId);
            //var group = groups.Where(k => k.Name == groupName);
            

            return user;
        }
    }
}