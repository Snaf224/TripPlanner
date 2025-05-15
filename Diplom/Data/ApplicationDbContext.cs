using Diplom.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Diplom.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripMember> TripMembers { get; set; }
        public DbSet<TravelTask> TravelTasks { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Не забудьте вызвать базовый метод для корректной работы Identity

            var passHasher = new PasswordHasher<ApplicationUser>();

            // Настройка данных для ApplicationUser
            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser()
                {
                    Id = "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4a",
                    UserName = "admin@email.em",
                    NormalizedUserName = "ADMIN@EMAIL.EM",
                    Email = "admin@email.em",
                    NormalizedEmail = "ADMIN@EMAIL.EM",
                    PasswordHash = passHasher.HashPassword(null!, "Password123++"),
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    Gender = "Male",
                },
                new ApplicationUser()
                {
                    Id = "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4b",
                    UserName = "user@email.em",
                    NormalizedUserName = "USER@EMAIL.EM",
                    Email = "user@email.em",
                    NormalizedEmail = "USER@EMAIL.EM",
                    PasswordHash = passHasher.HashPassword(null!, "Password123++"),
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    Gender = "Male",
                }
            );

            // Настройка ролей
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole()
                {
                    Id = "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4c",
                    Name = "admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                },
                new IdentityRole()
                {
                    Id = "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4d",
                    Name = "user",
                    NormalizedName = "USER",
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                }
            );

            // Связь пользователей с ролями
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>()
                {
                    UserId = "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4a",
                    RoleId = "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4c"
                },
                new IdentityUserRole<string>()
                {
                    UserId = "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4a",
                    RoleId = "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4d"
                },
                new IdentityUserRole<string>()
                {
                    UserId = "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4b",
                    RoleId = "49cd49b5-7b64-4c2f-8249-2fdf08b6ed4d"
                }
            );

            // Связи для других сущностей (Trip, TripMember, Expense и т.д.)
            // Trip → TripMember (One-to-Many)
            modelBuilder.Entity<TripMember>()
                .HasOne(tm => tm.Trip)
                .WithMany(t => t.Members)
                .HasForeignKey(tm => tm.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Trip → TravelTask (One-to-Many)
            modelBuilder.Entity<TravelTask>()
                .HasOne(t => t.Trip)
                .WithMany(t => t.Tasks)
                .HasForeignKey(t => t.TripId)
                .OnDelete(DeleteBehavior.NoAction);

            // Task → TripMember (One-to-Many, Optional)
            modelBuilder.Entity<TravelTask>()
                .HasOne(t => t.AssignedTo)
                .WithMany()
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

            // Trip → Expense (One-to-Many)
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Trip)
                .WithMany(t => t.Expenses)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Expense → TripMember (One-to-Many)
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.PaidBy)
                .WithMany()
                .HasForeignKey(e => e.PaidById)
                .OnDelete(DeleteBehavior.Restrict);

            // Trip → Notification (One-to-Many)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Trip)
                .WithMany(t => t.Notifications)
                .HasForeignKey(n => n.TripId)
                .OnDelete(DeleteBehavior.NoAction);

            // Notification → TripMember (One-to-Many, Optional)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Receiver)
                .WithMany()
                .HasForeignKey(n => n.ReceiverId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Trip>()
                .Property(t => t.Budget)
                .HasPrecision(18, 2);

            // TripMember.Contribution: до 18 цифр, 2 после запятой
            modelBuilder.Entity<TripMember>()
                .Property(tm => tm.Contribution)
                .HasPrecision(18, 2);

            // Expense.Amount: до 18 цифр, 2 после запятой
            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);
        }
    }
}
