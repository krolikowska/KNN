using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess
{
    /// <summary>
    ///     A data manager helpers.
    /// </summary>
    public class DataManagerHelpers
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public DataManagerHelpers()
        {
            ClearDb();
        }

        /// <summary>
        ///     Adds a user.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
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

        /// <summary>
        ///     Adds the users.
        /// </summary>
        /// <param name="userIds">
        ///     <see cref="List`1" /> of identifiers for the users.
        /// </param>
        public void AddUsers(int[] userIds)
        {
            foreach (var u in userIds) AddUser(u);
        }

        /// <summary>
        ///     Adds the books.
        /// </summary>
        /// <param name="isbn">The isbn.</param>
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

        /// <summary>
        ///     Adds the books ratings.
        /// </summary>
        /// <param name="rate">The rate.</param>
        public void AddBooksRatings(BooksRating rate)
        {
            using (var context = new BooksRecomendationsEntities())
            {
                context.BooksRatings.Add(rate);
                context.SaveChanges();
            }
        }

        /// <summary>
        ///     Gets books ratings for given user.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     The books ratings for given user.
        /// </returns>
        public List<BooksRating> GetBooksRatingsForGivenUser(int userId)
        {
            using (var context = new BooksRecomendationsEntities())
            {
                return context.BooksRatings.Where(x => x.UserId == userId).ToList();
            }
        }

        /// <summary>
        ///     Gets all book recomendadtion.
        /// </summary>
        /// <returns>
        ///     all book recomendadtion.
        /// </returns>
        public List<BookRecomendation> GetAllBookRecomendadtion()
        {
            using (var context = new BooksRecomendationsEntities())
            {
                return context.BookRecomendations.ToList();
            }
        }

        /// <summary>
        ///     Gets all similar users.
        /// </summary>
        /// <returns>
        ///     all similar users.
        /// </returns>
        public List<UserSimilar> GetAllSimilarUsers()
        {
            using (var context = new BooksRecomendationsEntities())
            {
                return context.UserSimilars.ToList();
            }
        }

        /// <summary>
        ///     <see cref="Random" /> number.
        /// </summary>
        /// <param name="seed">The seed.</param>
        /// <returns>
        ///     A double.
        /// </returns>
        public double RandomNumber(int seed)
        {
            var random = new Random(seed);
            return random.Next(0, 10);
        }

        /// <summary>
        ///     Clears the database.
        /// </summary>
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