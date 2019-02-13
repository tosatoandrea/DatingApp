using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Model;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            this._context = context;

        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            return await _context.Photos.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(x => x.Id == id);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();

            users = users.Where(x => x.Id != userParams.UserId && x.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(x => userLikers.Contains(x.Id));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(x => userLikees.Contains(x.Id));
            }

            var maxDateBirth = DateTime.Today.AddYears(-userParams.MinAge -1);
            var minDateBirth = DateTime.Today.AddYears(-userParams.MaxAge);

            users = users.Where(x => x.DateOfBirth >= minDateBirth && x.DateOfBirth <= maxDateBirth);

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch(userParams.OrderBy)
                {
                    case "created": 
                        users = users.OrderByDescending(u => u.Created); 
                        break;
                    default: 
                        users = users.OrderByDescending(u => u.LastActive); 
                        break;
                }
            }
        
            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int userId, bool likers)
        {
            var user = await _context.Users
                                .Include(x => x.Likers)
                                .Include(x => x.Likees)
                                .FirstOrDefaultAsync(u => u.Id == userId);

            if (likers)
            {
                return user.Likers.Where(u => u.LikeeId == userId).Select(u => u.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == userId).Select(u => u.LikeeId);
            }

        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.FirstOrDefaultAsync(x => x.IsMain && x.UserId == userId);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesFromUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                            .Include(x => x.Sender).ThenInclude(x => x.Photos)
                            .Include(x => x.Recipient).ThenInclude(x => x.Photos)
                            .AsQueryable();

            switch(messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(x => x.RecipientId == messageParams.UserId && x.RecipientHasDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(x => x.SenderId == messageParams.UserId && x.SenderHasDeleted == false);
                    break;
                default:
                    messages = messages.Where(x => x.RecipientId == messageParams.UserId && x.RecipientHasDeleted == false 
                        && x.IsRead == false);
                    break;
            }

            messages = messages.OrderBy(x => x.MessageSent);

            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
                            .Include(x => x.Sender).ThenInclude(x => x.Photos)
                            .Include(x => x.Recipient).ThenInclude(x => x.Photos)
                            .Where(x => (x.RecipientId == userId && !x.RecipientHasDeleted && x.SenderId == recipientId)
                                || (x.RecipientId == recipientId && x.SenderId == userId && !x.SenderHasDeleted))
                            .OrderByDescending(x => x.MessageSent)
                            .ToListAsync();
            return messages;
        }
    }
}