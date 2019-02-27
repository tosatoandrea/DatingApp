using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Model
{
    // usando IdentityUser<int> si dice a net core identity di usare un int come chiave primaria per l'untete invece che string come di default
    
    public class User : IdentityUser<int> 
    {
        // proprietà che esistono già nella classe base IdentityUser
        //public int Id { get; set; }
        //public byte[] PasswordHash { get; set; }
        //public string Username { get; set; }
        //public byte[] PasswordSalt { get; set; }

        public string Gender { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Introduction  { get; set; }
        public string LookingFor  { get; set; }
        public string Interests  { get; set; }
        public string City  { get; set; }
        public string Country  { get; set; }

        public ICollection<Photo> Photos { get; set; }
        public ICollection<Like> Likers { get; set; }
        public ICollection<Like> Likees { get; set; }

        public ICollection<Message> MessagesSent { get; set; }
        public ICollection<Message> MessagesReceived { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }
}