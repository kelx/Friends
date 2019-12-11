using MyFriendsApp.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MyFriendsApp.API.Data
{
    public class DataContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, 
        UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options){}
        public DbSet<Value> Values { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<RoleGroup> RoleGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);

            // builder.Entity<UserGroup>(userGroup => {

            //     userGroup.HasKey(ur => new {ur.Group.GroupId, ur.User.Id});
            //      userGroup.HasOne(ur => ur.Group)
            //       .WithMany(ur => ur.UserGroups)
            //       .HasForeignKey(ur => ur.Group.GroupId)
            //       .IsRequired();
            //      userGroup.HasOne(ur => ur.User)
            //       .WithMany(ur => ur.UserGroups)
            //       .HasForeignKey(ur => ur.User.Id)
            //       .IsRequired();
            // });

             // Add the group stuff here:
            // builder.Entity<User>().HasMany<Group>((User u) => u.UserGroups);
            // builder.Entity<UserGroup>().HasKey((UserGroup r) => new { r.UserId, r.GroupId });

            //  builder.Entity<Group>().HasMany<RoleGroup>((Group g) => g.GroupRoles);
            //  //builder.Entity<Group>().HasMany<UserGroup>((Group g) => g.GroupUsers);
            //  builder.Entity<RoleGroup>().HasKey((RoleGroup gr) => new { gr.RoleId, gr.GroupId });

            builder.Entity<UserGroup>(userGroup => {

                userGroup.HasKey(ur => new {ur.UserId, ur.GroupId});
                 userGroup.HasOne(ur => ur.Group)
                  .WithMany(ur => ur.GroupUsers)
                  .HasForeignKey(ur => ur.GroupId)
                  .IsRequired();
                 userGroup.HasOne(ur => ur.User)
                  .WithMany(gr => gr.UserGroups)
                  .HasForeignKey(ur => ur.UserId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();
            });
             builder.Entity<RoleGroup>(roleGroup => {

                roleGroup.HasKey(rg => new {rg.RoleId, rg.GroupId, rg.UserId});
                 roleGroup.HasOne(rg => rg.Group)
                  .WithMany(rg => rg.GroupRoles)
                  .HasForeignKey(ur => ur.GroupId)
                  .IsRequired();
                 roleGroup.HasOne(ur => ur.Role)
                  .WithMany(gr => gr.RoleGroups)
                  .HasForeignKey(ur => ur.RoleId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired();
                  
            });
            

            builder.Entity<UserRole>(userRole => {

                userRole.HasKey(ur => new {ur.UserId, ur.RoleId});
                 userRole.HasOne(ur => ur.Role)
                  .WithMany(ur => ur.UserRoles)
                  .HasForeignKey(ur => ur.RoleId)
                  .IsRequired();
                 userRole.HasOne(ur => ur.User)
                  .WithMany(ur => ur.UserRoles)
                  .HasForeignKey(ur => ur.UserId)
                  .IsRequired();
                  
            });


            builder.Entity<Like>().HasKey(k => new { k.LikerId, k.LikeeId });
            builder.Entity<Like>()
                .HasOne(k => k.Likee)
                .WithMany(k => k.Likers)
                .HasForeignKey(k => k.LikeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Like>()
            .HasOne(k => k.Liker)
            .WithMany(k => k.Likees)
            .HasForeignKey(k => k.LikerId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
            .HasOne(k => k.Sender)
            .WithMany(k => k.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Message>()
            .HasOne(k => k.Recipient)
            .WithMany(k => k.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Message>()
            .HasOne(k => k.GroupMessage)
            .WithMany(k => k.GroupMessages)
            .OnDelete(DeleteBehavior.Restrict);

        }
    }
}