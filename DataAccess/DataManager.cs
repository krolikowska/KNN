///-------------------------------------------------------------------------------------------------
// file:	DataManager.cs
//
// summary:	Implements the data manager class
///-------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace DataAccess
{
    ///-------------------------------------------------------------------------------------------------
    /// <summary>   Manager for data. </summary>
    ///
    /// <remarks>   Agnieszka, 11/01/2019. </remarks>
    ///-------------------------------------------------------------------------------------------------

    public class DataManager : IDataManager
    {
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets user from db by user id. </summary>
        ///
        /// <remarks>   Agnieszka, 11/01/2019. </remarks>
        ///
        /// <param name="UserId">   user id in database. </param>
        ///
        /// <returns>   User. </returns>
        ///-------------------------------------------------------------------------------------------------

        public User GetUser(int UserId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.Select(x => x.User).Where(x => x.UserId == UserId).FirstOrDefault();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Returns list of users from db for given set of ids. </summary>
        ///
        /// <remarks>   Agnieszka, 11/01/2019. </remarks>
        ///
        /// <param name="UserIds">  . </param>
        ///
        /// <returns>   The users. </returns>
        ///-------------------------------------------------------------------------------------------------

        public List<User> GetUsers(int[] UserIds)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.Select(x => x.User).Where(x => UserIds.Contains(x.UserId)).ToList();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets all users with rated books. </summary>
        ///
        /// <returns>   An array of user. </returns>
        ///-------------------------------------------------------------------------------------------------

        public User[] GetAllUsersWithRatedBooks()
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.Select(x => x.User).Distinct().ToArray();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets the users with nor more rated books in this collection. </summary>
        ///
        /// <remarks>   Agnieszka, 11/01/2019. </remarks>
        ///
        /// <param name="n">    Number of users who rated given book. </param>
        ///
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the users with nor more rated
        ///     books in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------

        public IEnumerable<User> GetUsersWithNorMoreRatedBooks(int n)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var temp = db.BooksRatings
                    .Select(x => x.User)
                    .Distinct()
                    .Where(x => x.BooksRatings.Count >= n).ToList();
                return temp;
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Returns books more popular books, rated by at least n users. </summary>
        ///
        /// <param name="ids">  Input set of books. </param>
        /// <param name="n">    Number of users who rated given book. </param>
        ///
        /// <returns>   Array of books ids. </returns>
        ///-------------------------------------------------------------------------------------------------

        public string[] GetBooksIdsRatedByAtLeastNusers(string[] ids, int n)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var books = db.BooksRatings.Where(x => ids.Contains(x.ISBN)).GroupBy(y => y.ISBN);
                return books.Where(x => x.Count() >= n).Select(x => x.Key).ToArray();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets books read by user. </summary>
        ///
        /// <param name="user"> The user. </param>
        ///
        /// <returns>   An array of book. </returns>
        ///-------------------------------------------------------------------------------------------------

        public Book[] GetBooksReadByUser(User user)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.Where(x => x.UserId == user.UserId).Select(z => z.Book).ToArray();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets books rated by user. </summary>
        ///
        /// <remarks>   Agnieszka, 11/01/2019. </remarks>
        ///
        /// <param name="user"> The user. </param>
        ///
        /// <returns>   An array of book. </returns>
        ///-------------------------------------------------------------------------------------------------

        public Book[] GetBooksRatedByUser(User user)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.Where(x => x.UserId == user.UserId).Select(x => x.Book).ToArray();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets books rates by user identifier. </summary>
        ///
        /// <remarks>   Agnieszka, 11/01/2019. </remarks>
        ///
        /// <param name="userId">   given user id. </param>
        ///
        /// <returns>   An array of book score. </returns>
        ///-------------------------------------------------------------------------------------------------

        public BookScore[] GetBooksRatesByUserId(int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.Where(x => x.UserId == userId).AsEnumerable()
                    .Select(x => new BookScore { BookId = x.ISBN, Rate = x.Rate }).ToArray();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets average rate for user. </summary>
        ///
        /// <param name="userId">   given user id. </param>
        ///
        /// <returns>   The average rate for user. </returns>
        ///-------------------------------------------------------------------------------------------------

        public double? GetAverageRateForUser(int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var ratings = db.BooksRatings.Where(x => x.UserId == userId).ToArray();
                if (ratings.Length == 0) return null;
                var average = ratings.Average(x => x.Rate);
                return Math.Round(average, 2);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets average rate for all books. </summary>
        ///
        /// <returns>   The average rate for all books. </returns>
        ///-------------------------------------------------------------------------------------------------

        public double GetAverageRateForAllBooks()
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var average = db.BooksRatings.Average(x => x.Rate);
                return Math.Round(average, 2);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets average rate for given book. </summary>
        ///
        /// <param name="isbn"> The isbn. </param>
        ///
        /// <returns>   The average rate for given book. </returns>
        ///-------------------------------------------------------------------------------------------------

        public double GetAverageRateForGivenBook(string isbn)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var average = db.BooksRatings.Where(x => x.ISBN == isbn).Average(x => x.Rate);
                return Math.Round(average, 2);
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets books identifiers same for both users. </summary>
        ///
        /// <param name="user1">    The first user. </param>
        /// <param name="user2">    The second user. </param>
        ///
        /// <returns>   An array of string. </returns>
        ///-------------------------------------------------------------------------------------------------

        public string[] GetBooksIdsSameForBothUsers(int user1, int user2)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var books1 = db.BooksRatings.Where(x => x.UserId == user1).Select(x => x.ISBN);
                var books2 = db.BooksRatings.Where(x => x.UserId == user2).Select(x => x.ISBN);

                return books1.Intersect(books2).Distinct().ToArray();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets books identifiers unique for second user. </summary>
        ///
        /// <param name="user1">    The first user. </param>
        /// <param name="user2">    The second user. </param>
        ///
        /// <returns>   An array of string. </returns>
        ///-------------------------------------------------------------------------------------------------

        public string[] GetBooksIdsUniqueForSecondUser(int user1, int user2)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var books1 = db.BooksRatings.Where(x => x.UserId == user1).Select(x => x.ISBN);
                var books2 = db.BooksRatings.Where(x => x.UserId == user2).Select(x => x.ISBN);
                return books2.Except(books1).Distinct().ToArray();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets users rates for given isbn list. </summary>
        ///
        /// <param name="isbn">     The isbn. </param>
        /// <param name="userId">   given user id. </param>
        ///
        /// <returns>   An array of book score. </returns>
        ///-------------------------------------------------------------------------------------------------

        public BookScore[] GetUsersRatesForGivenIsbnList(string[] isbn, int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var booksRates = db.BooksRatings
                    .Where(x => x.UserId == userId && isbn.Contains(x.ISBN)).AsEnumerable();
                return booksRates.Select(x => new BookScore { BookId = x.ISBN, Rate = x.Rate }).ToArray();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>
        ///     This procedure is updating all data about neigbors for specified user, at first its
        ///     deleting data we have and then adds new set.
        /// </summary>
        ///
        /// <param name="similarUsers">     list of users similar to given user. </param>
        /// <param name="userId">           given user id. </param>
        /// <param name="settingsVersion">  The settings version. </param>
        ///-------------------------------------------------------------------------------------------------

        public void AddSimilarUsers(List<UsersSimilarity> similarUsers, int userId, int settingsVersion)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                if (similarUsers.Count > 0)
                {
                    db.UserSimilars.AddRange(similarUsers.Select(x => MapToUserSimilarDataModel(x, settingsVersion)));
                }
                db.SaveChanges();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Adds a recommended books for user to 'userId'. </summary>
        ///
        /// <param name="books">    The books. </param>
        /// <param name="userId">   given user id. </param>
        ///-------------------------------------------------------------------------------------------------

        public void AddRecommendedBooksForUser(BookScore[] books, int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                //delete any if exist
                db.Database.ExecuteSqlCommand("Delete from BookRecomendation where UserId=@userId", new SqlParameter("@userId", userId));

                if (books != null)
                {
                    // insert new list
                    db.BookRecomendations.AddRange(books.Select(x => MapToBookRecomendationModel(x, userId)));
                }
                db.SaveChanges();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets recomennded books for user. </summary>
        ///
        /// <param name="userId">   given user id. </param>
        ///
        /// <returns>   An array of book. </returns>
        ///-------------------------------------------------------------------------------------------------

        public Book[] GetRecomenndedBooksForUser(int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BookRecomendations
                    .Where(x => x.UserId == userId)
                    .OrderByDescending(x => x.PredictedRate)
                    .Select(b => b.Book).ToArray();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets users neighbors. </summary>
        ///
        /// <param name="userId">           given user id. </param>
        /// <param name="settingsVersion">  The settings version. </param>
        ///
        /// <returns>   The users neighbors. </returns>
        ///-------------------------------------------------------------------------------------------------

        public List<UserSimilar> GetUsersNeighbors(int userId, int settingsVersion)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.UserSimilars
                    .Where(x => x.UserId == userId && x.ParametersSet == settingsVersion)
                    .ToList();
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Map to book recomendation model. </summary>
        ///
        /// <param name="book">     The book. </param>
        /// <param name="userId">   given user id. </param>
        ///
        /// <returns>   A BookRecomendation. </returns>
        ///-------------------------------------------------------------------------------------------------

        private BookRecomendation MapToBookRecomendationModel(BookScore book, int userId)
        {
            return new BookRecomendation
            {
                UserId = userId,
                BookId = book.BookId,
                PredictedRate = book.PredictedRate
            };
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Map to user similar data model. </summary>
        ///
        /// <param name="user">             The user. </param>
        /// <param name="settingsVersion">  The settings version. </param>
        ///
        /// <returns>   An UserSimilar. </returns>
        ///-------------------------------------------------------------------------------------------------

        private UserSimilar MapToUserSimilarDataModel(UsersSimilarity user, int settingsVersion)
        {
            return new UserSimilar
            {
                UserId = user.UserId,
                NeighborId = user.ComparedUserId,
                Similarity = user.Similarity,
                ParametersSet = settingsVersion
            };
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Saves the parameters set. </summary>
        ///
        /// <param name="parameter">    The parameter. </param>
        ///-------------------------------------------------------------------------------------------------

        public void SaveParametersSet(Parameter parameter)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var temp = db.Parameters.Find(parameter.Id);
                if (temp == null)
                {
                    db.Parameters.Add(parameter);
                }

                db.SaveChanges();
            }
        }
    }
}