using System.Collections.Generic;
using System.Linq;
using DatingApp.API.Model;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    public class Seed
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public Seed(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        

        public void SeedUsers()
        {
            if (!_userManager.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                var roles = new List<Role>
                {
                    new Role { Name = "Member" },
                    new Role { Name = "Admin" },
                    new Role { Name = "Moderator" },
                    new Role { Name = "VIP" }
                };

                foreach (var role in roles)
                    _roleManager.CreateAsync(role).Wait();

                foreach (var user in users)
                {
                    // siccome questo metodo non deve essere async, non si specifica asyn e await ma si usa .Wait()
                    // in questo modo il CreateAsync() è come se fosse sincrono
                    user.Photos.SingleOrDefault().IsApproved = true;
                    _userManager.CreateAsync(user, "password").Wait();
                    _userManager.AddToRoleAsync(user, "Member").Wait();
                }

                var adminUser = new User
                {
                    UserName = "Admin"
                };

                var identityResult = _userManager.CreateAsync(adminUser, "password").Result;
                if (identityResult.Succeeded)
                {
                    _userManager.AddToRolesAsync(adminUser, new[] { "Admin", "Moderator"}).Wait();
                }

            }
        }

    }
}