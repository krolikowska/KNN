using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace DataAccess
{
    /// <summary>
    ///     Manager for data.
    /// </summary>
    /// <inheritdoc />
    public class DataManager : IDataManager
    {
        /// <summary>
        ///     Gets a user.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     The user.
        /// </returns>
        public User GetUser(int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.Select(x => x.User).FirstOrDefault(x => x.UserId == userId);
            }
        }

        /// <summary>
        ///     Gets the users.
        /// </summary>
        /// <param name="userIds">
        ///  List of identifiers for the users.
        /// </param>
        /// <returns>
        ///     The users.
        /// </returns>
        public List<User> GetUsers(int[] userIds)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.AsNoTracking()
                         .Select(x => x.User)
                         .Where(x => userIds.Contains(x.UserId))
                         .ToList();
            }
        }

        /// <summary>
        ///     Gets users who rated given book.
        /// </summary>
        /// <param name="isbn">The isbn.</param>
        /// <returns>
        ///     The users who rated given book.
        /// </returns>
        public List<BooksRating> GetBookRatingsForUsersWhoRatedGivenBook(string isbn)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.Where(x => x.ISBN == isbn).ToList();
            }
        }

        /// <summary>
        ///     Gets all users with rated books.
        /// </summary>
        /// <returns>
        ///     An array of user.
        /// </returns>
        public User[] GetAllUsersWithRatedBooks()
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.AsNoTracking().Select(x => x.User).Distinct().ToArray();
            }
        }

        /// <summary>
        ///     Gets the users with nor more rated books in this collection.
        /// </summary>
        /// <param name="n">An <see langword="int" /> to process.</param>
        /// <returns>
        ///     An enumerator that allows <see langword="foreach" /> to be used to
        ///     process the users with nor more rated books in this collection.
        /// </returns>
        public IEnumerable<User> GetUsersWithNorMoreRatedBooks(int n)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var temp = db.BooksRatings.AsNoTracking()
                             .Select(x => x.User)
                             .Distinct()
                             .Where(x => x.BooksRatings.Count >= n)
                             .ToList();
                return temp;
            }
        }

        public List<int> GetUsersIdsWithNorMoreRatedBooks(int n)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var temp = db.BooksRatings
                             .Where(x => x.User.BooksRatings.Count >= n)
                             .Select(x => x.UserId)
                             .Distinct()
                             .OrderBy(x => x)
                             .ToList();

                return temp;
            }
        }

        /// <summary>
        ///     Gets the books identifiers rated by at least n users in this
        ///     collection.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <param name="n">An <see langword="int" /> to process.</param>
        /// <returns>
        ///     An enumerator that allows <see langword="foreach" /> to be used to
        ///     process the books identifiers rated by at least n users in this
        ///     collection.
        /// </returns>
        public string[] GetBooksIdsRatedByAtLeastNUsers(string[] ids, int n)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var books = db.BooksRatings.AsNoTracking().Where(x => ids.Contains(x.ISBN)).GroupBy(y => y.ISBN);
                return books.Where(x => x.Count() >= n).Select(x => x.Key).ToArray();
            }
        }

        /// <summary>
        ///     Gets the books identifiers rated by at least n users in this
        ///     collection.
        /// </summary>
        /// <param name="n">An <see langword="int" /> to process.</param>
        /// <returns>
        ///     An enumerator that allows <see langword="foreach" /> to be used to
        ///     process the books identifiers rated by at least n users in this
        ///     collection.
        /// </returns>
        public IEnumerable<string> GetBooksIdsRatedByAtLeastNUsers(int n)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var books = db.BooksRatings.AsNoTracking().GroupBy(y => y.ISBN);
                return books.Where(x => x.Count() >= n).OrderByDescending(x => x.Count()).Select(x => x.Key).ToArray();
            }
        }

        /// <summary>
        ///     Gets books read by user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///     An array of book.
        /// </returns>
        public Book[] GetBooksReadByUser(User user)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.AsNoTracking().Where(x => x.UserId == user.UserId).Select(z => z.Book).ToArray();
            }
        }

        /// <summary>
        ///     Gets books rated by user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///     An array of book.
        /// </returns>
        public Book[] GetBooksRatedByUser(User user)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.AsNoTracking().Where(x => x.UserId == user.UserId).Select(x => x.Book).ToArray();
            }
        }

        /// <summary>
        ///     Gets books rates by user identifier.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     An array of book score.
        /// </returns>
        public BookScore[] GetBooksRatesByUserId(int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.AsNoTracking()
                         .Where(x => x.UserId == userId)
                         .AsEnumerable()
                         .Select(x => new BookScore {BookId = x.ISBN, Rate = x.Rate, UserId = userId})
                         .ToArray();
            }
        }

        /// <summary>
        ///     Gets average rate for user.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     The average rate for user.
        /// </returns>
        public double? GetAverageRateForUser(int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var ratings = db.BooksRatings.AsNoTracking().Where(x => x.UserId == userId).ToArray();
                if (ratings.Length == 0) return null;
                var average = ratings.Average(x => x.Rate);
                return Math.Round(average, 2);
            }
        }

        /// <summary>
        ///     Gets average rate for all books.
        /// </summary>
        /// <returns>
        ///     The average rate for all books.
        /// </returns>
        public double GetAverageRateForAllBooks()
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var average = db.BooksRatings.AsNoTracking().Average(x => x.Rate);
                return Math.Round(average, 2);
            }
        }

        /// <summary>
        ///     Gets average rate for given book.
        /// </summary>
        /// <param name="isbn">The isbn.</param>
        /// <returns>
        ///     The average rate for given book.
        /// </returns>
        public double GetAverageRateForGivenBook(string isbn)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var average = db.BooksRatings.AsNoTracking().Where(x => x.ISBN == isbn).Average(x => x.Rate);
                return Math.Round(average, 2);
            }
        }

        /// <summary>
        ///     Gets books identifiers same for both users.
        /// </summary>
        /// <param name="user1">The first user.</param>
        /// <param name="user2">The second user.</param>
        /// <returns>
        ///     An array of string.
        /// </returns>
        public string[] GetBooksIdsSameForBothUsers(int user1, int user2)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var books1 = db.BooksRatings.AsNoTracking().Where(x => x.UserId == user1).Select(x => x.ISBN);
                var books2 = db.BooksRatings.AsNoTracking().Where(x => x.UserId == user2).Select(x => x.ISBN);

                return books1.Intersect(books2).Distinct().ToArray();
            }
        }

        /// <summary>
        ///     Gets books identifiers unique for second user.
        /// </summary>
        /// <param name="user1">The first user.</param>
        /// <param name="user2">The second user.</param>
        /// <returns>
        ///     An array of string.
        /// </returns>
        public string[] GetBooksIdsUniqueForSecondUser(int user1, int user2)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var books1 = db.BooksRatings.AsNoTracking().Where(x => x.UserId == user1).Select(x => x.ISBN);
                var books2 = db.BooksRatings.AsNoTracking().Where(x => x.UserId == user2).Select(x => x.ISBN);
                return books2.Except(books1).Distinct().ToArray();
            }
        }

        /// <summary>
        ///     Gets users rates for given <paramref name="isbn" /> list.
        /// </summary>
        /// <param name="isbn">The isbn.</param>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     An array of book score.
        /// </returns>
        public BookScore[] GetUsersRatesForGivenIsbnList(string[] isbn, int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var booksRates = db.BooksRatings.AsNoTracking()
                                   .Where(x => x.UserId == userId && isbn.Contains(x.ISBN))
                                   .AsEnumerable();
                return booksRates.Select(x => new BookScore {BookId = x.ISBN, Rate = x.Rate}).ToArray();
            }
        }

        /// <summary>
        ///     Adds a similar users.
        /// </summary>
        /// <param name="similarUsers">The similar users.</param>
        /// <param name="userId">Identifier for the user.</param>
        /// <param name="settingsVersion">The settings version.</param>
        public void AddSimilarUsers(List<UsersSimilarity> similarUsers, int userId, int settingsVersion)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                if (similarUsers.Count > 0)

                    db.UserSimilars.AddRange(similarUsers.Select(x => MapToUserSimilarDataModel(x, settingsVersion)));
                db.SaveChanges();
            }
        }

        /// <summary>
        ///     Adds a recommended <paramref name="books" /> for user to 'userId'.
        /// </summary>
        /// <param name="books">The books.</param>
        /// <param name="userId">Identifier for the user.</param>
        public void AddRecommendedBooksForUser(BookScore[] books, int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                //delete any if exist
                db.Database.ExecuteSqlCommand("Delete from BookRecomendation where UserId=@userId",
                                              new SqlParameter("@userId", userId));

                if (books != null)
                    db.BookRecomendations.AddRange(books.Select(x => MapToBookRecommendationModel(x, userId)));

                db.SaveChanges();
            }
        }

        public void AddTestResults(List<BookScore[]> scores, int settingVersion)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var result = new List<Test>();
                foreach (var score in scores)
                    result.AddRange(score.Select(bookScore => MapScoreToTest(bookScore, settingVersion)));

                db.Tests.AddRange(result);
                db.SaveChanges();
            }
        }

        private Test MapScoreToTest(BookScore score, int settingVersion)
        {
            return new Test
            {
                ActualRate = score.Rate,
                BookId = score.BookId,
                ParametersSet = settingVersion,
                UserId = score.UserId,
                PredictedRate = score.PredictedRate
            };
        }

        /// <summary>
        ///     Gets recomennded books for user.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     An array of book.
        /// </returns>
        public Book[] GetRecommendedBooksForUser(int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BookRecomendations
                         .Where(x => x.UserId == userId)
                         .OrderByDescending(x => x.PredictedRate)
                         .Select(b => b.Book)
                         .ToArray();
            }
        }

        /// <summary>
        ///     Gets users neighbors.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <param name="settingsVersion">The settings version.</param>
        /// <returns>
        ///     The users neighbors.
        /// </returns>
        public List<UserSimilar> GetUsersNeighbors(int userId, int settingsVersion)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.UserSimilars.AsNoTracking()
                         .Where(x => x.UserId == userId && x.ParametersSet == settingsVersion)
                         .ToList();
            }
        }

        public int[] GetUsersWithComputedSimilarity(int settingVersion)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var z = db.UserSimilars.AsNoTracking()
                          .Where(x => x.ParametersSet == settingVersion)
                          .Select(x => x.UserId)
                          .Distinct()
                          .ToArray();

                return z;
            }
        }

        /// <summary>
        ///     Saves the parameters set.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void SaveParametersSet(Parameter parameter)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var temp = db.Parameters.Find(parameter.Id);
                if (temp == null) db.Parameters.Add(parameter);

                db.SaveChanges();
            }
        }

        public Parameter GetParameters(int settingsVersion)
        {
            using (var db = new BooksRecomendationsEntities())
            {

                var settings = db.Parameters.Where(x => x.Id == settingsVersion).ToList();
                return settings.Select(x =>
                                   new Parameter
                                   {
                                       BookPopularity = x.BookPopularity,
                                       DistanceSimilarityType = x.DistanceSimilarityType,
                                       DistanceType = x.DistanceType,
                                       Id = x.Id,
                                       Kneigbors = x.Kneigbors,
                                       NumberOfBooksEachUserRated = x.NumberOfBooksEachUserRated
                                   })
                    .FirstOrDefault();
            }
        }

        /// <summary>
        ///     Gets users who rated any of given books.
        /// </summary>
        /// <param name="bookIds">
        ///     <see cref="List`1" /> of identifiers for the books.
        /// </param>
        /// <returns>
        ///     An array of int.
        /// </returns>
        public int[] GetUsersWhoRatedAnyOfGivenBooks(string[] bookIds)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.AsNoTracking()
                         .Where(x => bookIds.Contains(x.ISBN))
                         .Select(x => x.UserId)
                         .Distinct()
                         .ToArray();
            }
        }

        /// <summary>
        ///     Map to <paramref name="book" /> recommendation model.
        /// </summary>
        /// <param name="book">.</param>
        /// <param name="userId">.</param>
        /// <returns>
        ///     A BookRecomendation.
        /// </returns>
        private static BookRecomendation MapToBookRecommendationModel(BookScore book, int userId)
        {
            return new BookRecomendation
            {
                UserId = userId,
                BookId = book.BookId,
                PredictedRate = book.PredictedRate
            };
        }

        /// <summary>
        ///     Map to <paramref name="user" /> similar data model.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="settingsVersion">The settings version.</param>
        /// <returns>
        ///     An UserSimilar.
        /// </returns>
        private static UserSimilar MapToUserSimilarDataModel(UsersSimilarity user, int settingsVersion)
        {
            var s = new UserSimilar
            {
                UserId = user.UserId,
                NeighborId = user.ComparedUserId,
                Similarity = user.Similarity,
                ParametersSet = settingsVersion
            };

            return s;
        }

        public List<SelectMutualBooks_Result> GetMutualBooksForUsers(int user1, int user2)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.SelectMutualBooks(user1, user2).ToList();
            }
        }
    }
}