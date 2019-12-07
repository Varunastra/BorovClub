using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BorovClub.Models;

namespace BorovClub.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Message> Messages { get; set; }

        public DbSet<Chat> Chats { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Message>().HasKey(f => f.MessageId );

            builder.Entity<Message>()
                .HasOne(m => m.Reciever)
                .WithMany()
                .HasForeignKey(m => m.RecieverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Friendship>().HasKey(f => new { f.SenderId, f.RecieverId});

            builder.Entity<Friendship>()
                .HasOne(u => u.Reciever)
                .WithMany()
                .HasForeignKey(f => f.RecieverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Friendship>()
                .HasOne(f => f.Sender)
                .WithMany()
                .HasForeignKey(f => f.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Chat>().HasKey(c => new { c.SenderId, c.RecieverId });

            builder.Entity<Chat>()
                .HasOne(c => c.Sender)
                .WithMany()
                .HasForeignKey(u => u.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Chat>()
                .HasOne(c => c.Reciever)
                .WithMany()
                .HasForeignKey(u => u.RecieverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Chat>()
                .HasOne(c => c.LastMessage)
                .WithMany()
                .HasForeignKey(c => c.LastMessageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
