using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;

namespace DataAccess.Tests
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

                context.Users.AddOrUpdate(user);
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
                        BookAuthor = "Author_" + i,
                        Publisher = "Editor",
                        YearOfPublication = 2012
                    };
                    context.Books.AddOrUpdate(book);
                }

                context.SaveChanges();
            }
        }

        public void AddParameters(int id)
        {
            using (var context = new BooksRecomendationsEntities())
            {
                context.DistanceSimilarityTypes.AddOrUpdate(new DistanceSimilarityType {Id = 1});
                context.Parameters.AddOrUpdate(new Parameter {Id = id, DistanceType = 1});
                context.SaveChanges();
            }
        }

        public void AddDistanceSimilarityTypes(int id)
        {
            using (var context = new BooksRecomendationsEntities())
            {
                context.DistanceSimilarityTypes.AddOrUpdate(new DistanceSimilarityType { Id = id });
                context.SaveChanges();
            }
        }

        public void AddBooksRatings(List<BooksRating> rates)
        {
            var users = rates.Select(x => x.UserId).Distinct().ToArray();
            var books = rates.Select(x => x.ISBN).Distinct().ToArray();

            AddUsers(users);
            AddBooks(books);

            using (var context = new BooksRecomendationsEntities())
            {
                context.BooksRatings.AddRange(rates);
                context.SaveChanges();
            }
        }

        public List<BookRecomendation> GetAllBookRecomendadtion()
        {
            using (var context = new BooksRecomendationsEntities())
            {
                return context.BookRecomendations.ToList();
            }
        }

        public List<UserSimilar> GetAllSimilarUsers(int parameterSetId)
        {
            using (var context = new BooksRecomendationsEntities())
            {
                return context.UserSimilars.Where(x => x.ParametersSet == parameterSetId).Distinct().ToList();
            }
        }

        public double RandomNumber(int seed)
        {
            var random = new Random(seed);
            return random.Next(0, 10);
        }

        public List<UsersSimilarity> CreateSimilarUsers(int userId, int[] userNeighbors, int parametersId)
        {
            AddUser(userId);
            AddUsers(userNeighbors);
            AddParameters(parametersId);

            return userNeighbors
                   .Select(neighbor => new UsersSimilarity {UserId = userId, ComparedUserId = neighbor})
                   .ToList();
        }

        public BookScore[] CreateBookScoreBasedOnUser(int userId, string[] isbn, IReadOnlyList<short> rates)
        {
            AddBooksRatedByUser(userId, isbn, rates);
            return isbn.Select((id, i) => new BookScore
                       {
                           BookId = id,
                           Rate = rates[i],
                           PredictedRate = RandomNumber(i),
                           UserId = userId
                       })
                       .ToArray();
        }

        public void AddBooksRatedByUser(int userId, IEnumerable<string> isbn, IReadOnlyList<short> rates)
        {
            var booksRatings = isbn.Select((id,
                                               i) => new BooksRating
                                               {
                                                   ISBN = id,
                                                   UserId = userId,
                                                   Rate = rates[i]
                                               })
                                   .ToList();
            AddBooksRatings(booksRatings);
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
                context.Parameters.Clear();
                context.DistanceSimilarityTypes.Clear();
                context.SaveChanges();
            }
        }
    }
}