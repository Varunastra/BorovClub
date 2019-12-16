using BorovClub.Data;
using BorovClub.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BorovClub.Tests
{
    public static class Initializer
    {
        private readonly static List<ApplicationUser> fakeUsers = new List<ApplicationUser>();
        private readonly static List<Friendship> fakeFriendships = new List<Friendship>();
        private readonly static List<Message> fakeMessages = new List<Message>();
        private readonly static List<Chat> fakeChats = new List<Chat>();

        static Initializer()
        {
            fakeUsers.AddRange(new [] {
                new ApplicationUser
                {
                    UserName = "Varunastra",
                    Email = "dyadyavaleras@gmail.com",
                    FirstName = "Konstantsin",
                    LastName = "Kolasau",
                    TotpSecret = "TESQ===="
                },
                new ApplicationUser
                {
                    UserName = "Valera",
                    Email = "valera@yandex.by",
                    FirstName = "Valera",
                    LastName = "Kimovsk"
                }
            }
            );

            fakeFriendships.AddRange(new[] {
                   new Friendship
                   {
                       Sender = fakeUsers[0],
                       Reciever = fakeUsers[1],
                       Status = FriendshipStatus.Approved
                   },
                   new Friendship
                   {
                       Sender = fakeUsers[1],
                       Reciever = fakeUsers[0],
                       Status = FriendshipStatus.Approved
                   }
               }
            );

            fakeMessages.AddRange(new[] {
                    new Message
                    {
                        Reciever = fakeUsers[0],
                        Sender = fakeUsers[1],
                        Text = "testA",
                        When = DateTime.UtcNow
                    },
                    new Message
                    {
                        Reciever = fakeUsers[1],
                        Sender = fakeUsers[0],
                        Text = "testB",
                        When = DateTime.UtcNow
                    }
                }
             );

            fakeChats.AddRange(new[] {
                    new Chat
                    {
                        LastMessage = fakeMessages[0],
                        Sender = fakeMessages[0].Sender,
                        Reciever = fakeMessages[0].Reciever
                    },
                    new Chat
                    {
                        LastMessage = fakeMessages[1],
                        Sender = fakeMessages[1].Sender,
                        Reciever = fakeMessages[1].Reciever
                    }
                }
            );
        }
        public static void InitializeDbForTests(ApplicationDbContext dbContext)
        {
            dbContext.Users.AddRange(fakeUsers);
            dbContext.Friendships.AddRange(fakeFriendships);
            dbContext.Messages.AddRange(fakeMessages);
            dbContext.Chats.AddRange(fakeChats);
            dbContext.SaveChanges();
        }
    }
}
