using System;
using System.Collections.Generic;
using System.Linq;

namespace bookRecomendationKnn
{
    public class DataManager : IDataManager
    {
        public List<User> GetAllUsersWithRatedBooks()
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.Select(x => x.User).Distinct().ToList();
            }
        }

        public List<User> GetUsersWithMoreThanNratedBooks(int n)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings
                    .Select(x => x.User)
                    .Distinct()
                    .Where(x => x.BooksRatings.Count >= n).ToList();
            }
        }

        public void GetStDevScore(out double mean, out double stdDev)
        {
            mean = 0.0;
            stdDev = 0.0;

            using (var db = new BooksRecomendationsEntities())
            {
                var values = db.BooksRatings.Select(x => x.Rate);
                double sum = 0.0;

                int n = 0;
                foreach (double val in values)
                {
                    n++;
                    double delta = val - mean;
                    mean += delta / n;
                    sum += delta * (val - mean);
                }
                if (1 < n)
                    stdDev = Math.Sqrt(sum / (n - 1));
            }
        }

        public List<Book> GetBooksReadByUser(User user)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.Where(x => x.UserId == user.UserId).Select(z => z.Book).ToList();
            }
        }

        public List<Book> GetBooksRatedByUser(User user)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.Where(x => x.UserId == user.UserId).Select(x => x.Book).ToList();
            }
        }

        public List<BookRates> GetBooksRatesByUserId(int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BooksRatings.Where(x => x.UserId == userId).AsEnumerable()
                    .Select(x => new BookRates { BookId = x.ISBN, Rate = x.Rate }).ToList();
            }
        }

        public double GetAverageRateForUser(int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {

                var average = db.BooksRatings.Where(x => x.UserId == userId).Average(x => x.Rate);
                return Math.Round(average, 0);
            }
        }

        public string[] GetBooksIdsSameForBothUsers(int user1, int user2)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var books1 = db.BooksRatings.Where(x => x.UserId == user1).Select(x => x.ISBN);
                var books2 = db.BooksRatings.Where(x => x.UserId == user2).Select(x => x.ISBN);

                return books1.Intersect(books2).Distinct().ToArray();
            }
        }

        public string[] GetBooksIdsUniqueForSecondUser(int user1, int user2)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var books1 = db.BooksRatings.Where(x => x.UserId == user1).Select(x => x.ISBN);
                var books2 = db.BooksRatings.Where(x => x.UserId == user2).Select(x => x.ISBN);
                return books2.Except(books1).Distinct().ToArray();
            }
        }

        public List<BookRates> GetUsersRatesForGivenIsbnList(string[] isbn, int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                var booksRates = db.BooksRatings
                    .Where(x => x.UserId == userId && isbn.Contains(x.ISBN)).AsEnumerable();
                return booksRates.Select(x => new BookRates { BookId = x.ISBN, Rate = x.Rate }).ToList();
            }
        }

        /// <summary>
        /// This procedure is updating all data about neigbors for specified user, at first its deleting data we have and then adds new set
        /// </summary>
        /// <param name="similarUsers">list of users similar to given user</param>
        /// <param name="userId">given user id</param>
        public void AddSimilarUsers(List<UsersSimilarity> similarUsers, int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                //delete if exist
                var currentNeighbors = db.UserSimilars.Where(x => x.UserId == userId).ToList();
                db.UserSimilars.RemoveRange(currentNeighbors);
                // insert new list
                db.UserSimilars.AddRange(similarUsers.Select(x => MapToUserSimilarDataModel(x)));
                db.SaveChanges();
            }
        }

        public void AddRecommendedBooksForUser(List<BookRates> books, int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                //delete if exist
                var currenRecommendadtions = db.BookRecomendations.Where(x => x.UserId == userId).ToList();
                db.BookRecomendations.RemoveRange(currenRecommendadtions);
                // insert new list
                db.BookRecomendations.AddRange(books.Select(x => MapToBookRecomendationModel(x, userId)));
                db.SaveChanges();
            }
        }

        public List<Book> GetRecomenndedBooksForUser(int userId)
        {
            using (var db = new BooksRecomendationsEntities())
            {
                return db.BookRecomendations.Where(x => x.UserId == userId).Select(b => b.Book).ToList();
            }
        }


        private BookRecomendation MapToBookRecomendationModel(BookRates book, int userId)
        {
            return new BookRecomendation
            {
                UserId = userId,
                BookId = book.BookId,
                PredictedRate = book.RatePredictionCoeficient
            };
        }

        private UserSimilar MapToUserSimilarDataModel(UsersSimilarity user)
        {
            return new UserSimilar
            {
                UserId = user.UserId,
                NeighborId = user.ComparedUserId,
                Similarity = user.Similarity


            };
        }
        public UserInfo CreateUserInfo(int userId)
        {
            return new UserInfo
            {
                UserId = userId,
                AllReadBooks = GetBooksRatesByUserId(userId)
            };
        }

    

    }
}