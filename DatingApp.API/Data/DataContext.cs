using DatingApp.API.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public class DataContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, 
                                IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DbSet<Value> Values { get; set; }
        
        // non serve più perchè viene ereditata dalla classe base Identity
        //public DbSet<User> Users { get; set; }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // dato che si eredita da IdentityDbContext occorre chiamare il seguente metodo della classe base
            base.OnModelCreating(builder);

            // serve per definire le relazioni molti a molti tra utenti e ruoli
            builder.Entity<UserRole>(userRole => 
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
                
            });

            builder.Entity<Like>()
                .HasKey(k => new { k.LikerId, k.LikeeId } );
            
            builder.Entity<Like>()
                .HasOne(u => u.Likee)
                .WithMany(u => u.Likers)
                .HasForeignKey(u => u.LikeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Like>()
                .HasOne(u => u.Liker)
                .WithMany(u => u.Likees)
                .HasForeignKey(u => u.LikerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);    
                
            builder.Entity<Message>()
                .HasOne(u => u.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Photo>()
                .Property(p => p.IsApproved)
                .HasDefaultValue(false);

            // di default ritorna solo le foto già approvate,
            // per ignorare il filtro occorre richiamare la query con .IgnoreQueryFilters()
            builder.Entity<Photo>()
                .HasQueryFilter(p => p.IsApproved);
        }

        public DataContext(DbContextOptions<DataContext> options) :
            base(options)
        {

        }

        
    }
}
