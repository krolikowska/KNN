using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace DataAccess
{
    public class DataManager : IDataManager
    {
        public User[] GetAllUsersWithRatedBooks()
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.AsNoTracking().Select(x => x.User).Distinct().ToArray();
            }
        }

        public List<int> GetUserIdsWithNorMoreRatedBooks(int n)
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

        public string[] GetBooksIdsRatedByAtLeastNUsers(string[] ids, int n)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var books = db.BooksRatings.AsNoTracking().Where(x => ids.Contains(x.ISBN)).GroupBy(y => y.ISBN);
                return books.Where(x => x.Count() >= n).Select(x => x.Key).ToArray();
            }
        }

        public IEnumerable<string> GetBooksIdsRatedByAtLeastNUsers(int n)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var books = db.BooksRatings.AsNoTracking().GroupBy(y => y.ISBN);
                return books.Where(x => x.Count() >= n).OrderByDescending(x => x.Count()).Select(x => x.Key).ToArray();
            }
        }

        public Book[] GetBooksReadByUser(User user)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.AsNoTracking().Where(x => x.UserId == user.UserId).Select(z => z.Book).ToArray();
            }
        }

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

        public void AddSimilarUsers(List<UsersSimilarity> similarUsers, int settingsVersion)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var result = similarUsers.Select(x => MapToUserSimilarDataModel(x, settingsVersion));
                db.UserSimilars.AddRange(result);
                db.SaveChanges();
            }
        }

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

        public Book[] GetBooksFromGivenBookScores(IEnumerable<string> bookIds)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.Books.Where(x => bookIds.Contains(x.ISBN)).ToArray();
            }
        }

        public List<UserSimilar> GetUserNeighbors(int userId, int settingsVersion)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.UserSimilars.AsNoTracking()
                         .Where(x => x.UserId == userId && x.ParametersSet == settingsVersion)
                         .ToList();
            }
        }

        public int[] GetAllUsersWithComputedSimilarity(int settingVersion)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.UserSimilars.AsNoTracking()
                         .Where(x => x.ParametersSet == settingVersion)
                         .Select(x => x.UserId)
                         .Distinct()
                         .ToArray();
            }
        }

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

        public List<int> GetUsersWhoRatedAnyOfGivenBooks(string[] bookIds)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.AsNoTracking()
                         .Where(x => bookIds.Contains(x.ISBN))
                         .Select(x => x.UserId)
                         .Distinct()
                         .ToList();
            }
        }

        private static BookRecomendation MapToBookRecommendationModel(BookScore book, int userId)
        {
            return new BookRecomendation
            {
                UserId = userId,
                BookId = book.BookId,
                PredictedRate = book.PredictedRate
            };
        }

        private static UserSimilar MapToUserSimilarDataModel(UsersSimilarity user, int settingsVersion)
        {
            return new UserSimilar
            {
                UserId = user.UserId,
                NeighborId = user.ComparedUserId,
                Similarity = user.Similarity,
                ParametersSet = settingsVersion
            };
        }
    }
}