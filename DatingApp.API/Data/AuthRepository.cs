using System;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Model;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _cntx;
        public AuthRepository(DataContext cntx)
        {
            _cntx = cntx;
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await _cntx.Users.Include(x => x.Photos).FirstOrDefaultAsync(x => x.Username == username);

            if (user == null)
                return null;

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) 
                return null;

            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt)) 
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                //return computedHash.SequenceEqual(passwordHash);

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                        return false;
                }
                return true;
            }
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            GeneratePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _cntx.Users.AddAsync(user);
            await _cntx.SaveChangesAsync();

            return user;
        }

        private void GeneratePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512()) 
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string username)
        {
            if (await _cntx.Users.AnyAsync(x => x.Username == username))
                return true;

            return false;
        }
    }
}