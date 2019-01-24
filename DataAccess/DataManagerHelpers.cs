using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess
{
    public class DataManagerHelpers
    {
        public DataManagerHelpers()
        {
            ClearDb();
        }

        public void AddUser(int userId)
        {
            using (var context = new BooksRecomendationsEntities())
            {
                var user = new User
                {
                    UserId = userId,
                    Name = "Name_" + userId,
                    Surname = "Lastname_" + userId,
                    Age = 21,
                    Location = "City_" + userId
                };

                context.Users.Add(user);
                context.SaveChanges();
            }
        }

  
        public void AddUsers(int[] userIds)
        {
            foreach (var u in userIds) AddUser(u);
        }

    
        public void AddBooks(string[] isbn)
        {
            using (var context = new BooksRecomendationsEntities())
            {
                for (var i = 0; i < isbn.Length; i++)
                {
                    var book = new Book
                    {
                        ISBN = isbn[i],
                        BookTitle = "Title" + i,
                        ImageURLL = "big" + isbn[i],
                        ImageURLM = "medium" + isbn[i],
                        ImageURLS = "small" + isbn[i],
                        BookAuthor = "Auhor_" + i,
                        Publisher = "Editor",
                        YearOfPublication = 2012
                    };
                    context.Books.Add(book);
                }

                context.SaveChanges();
            }
        }

       
        public void AddBooksRatings(BooksRating rate)
        {
            using (var context = new BooksRecomendationsEntities())
            {
                context.BooksRatings.Add(rate);
                context.SaveChanges();
            }
        }

        public List<BooksRating> GetBooksRatingsForGivenUser(int userId)
        {
            using (var context = new BooksRecomendationsEntities())
            {
                return context.BooksRatings.Where(x => x.UserId == userId).ToList();
            }
        }

        public List<BookRecomendation> GetAllBookRecomendadtion()
        {
            using (var context = new BooksRecomendationsEntities())
            {
                return context.BookRecomendations.ToList();
            }
        }

        public double RandomNumber(int seed)
        {
            var random = new Random(seed);
            return random.Next(0, 10);
        }

        public void ClearDb()
        {
            using (var context = new BooksRecomendationsEntities())
            {
                context.UserSimilars.Clear();
                context.BookRecomendations.Clear();
                context.BooksRatings.Clear();
                context.Books.Clear();
                context.Users.Clear();
                context.SaveChanges();
            }
        }
    }
}