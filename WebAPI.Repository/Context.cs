using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using WebAPI.Dominio;

namespace WebAPI.Repository
{
    public class Context : IdentityDbContext<User, Role, int,
                                             IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>,
                                             IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public Context(DbContextOptions<Context> options) : base(options) { }

        public DbSet<Registro> RecordContext { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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

            builder.Entity<Registro>( r =>{
                r.HasOne(re => re.User)
                .WithMany(u => u.Registros)
                .IsRequired();

            });

           
        }

       
    }
}
