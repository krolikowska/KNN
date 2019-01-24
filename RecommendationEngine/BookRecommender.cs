using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess;
using RecommendationEngine.Properties;

namespace RecommendationEngine
{
    public class BookRecommender : IBookRecommender
    {
        private readonly ICommon _common;
        private readonly int _numbOfBooksToRecommend;
        private readonly ISettings _settings;
        public BookRecommender(ISettings settings, ICommon common)
        {
            _settings = settings;
            _common = common;
            _numbOfBooksToRecommend = settings.NumOfBooksToRecommend;
        }

        public BookScore[] GetRecommendedBooks(List<UsersSimilarity> similarUsers, int userId)
        {
            var booksIds = _common.PreparePotentialBooksToRecommendation(similarUsers, userId);
            var booksRates = GetAllRecommendedBooksForUser(similarUsers, userId, booksIds);
            var result = booksRates.Take(_numbOfBooksToRecommend).ToArray();
            _common.PersistRecommendedBooksInDb(result, userId);
            return result.ToArray();
        }

        public SortedSet<BookScore> GetAllRecommendedBooksForUser(List<UsersSimilarity> similarUsers, int userId,
            string[] booksIds)
        {
            if (similarUsers == null || similarUsers?.Count == 0) return null;
            var recommendedBooks = new SortedSet<BookScore>(new BookScoreComparer());

            if (booksIds.Length == 0)
            {
                _common.PersistRecommendedBooksInDb(null, userId);
                return null;
            }


            var meanRateForUser = _common.GetAverageRateForUser(userId) ?? 0;

            foreach (var id in booksIds)
            {
                var temp = new BookScore
                {
                    UserId = userId,
                    BookId = id
                };
                var bookScore = EvaluateScore(similarUsers, temp, meanRateForUser);
                if (bookScore != null)
                    recommendedBooks.Add(bookScore);
            }

            return recommendedBooks;
        }

        public List<UsersSimilarity> CombineUniqueAndMutualBooksForUser(List<UsersSimilarity> similarUsers)
        {
            foreach (var u in similarUsers)
                u.BooksUniqueForComparedUser =
                    u.BooksUniqueForComparedUser.Concat(u.ComparedUserRatesForMutualBooks).ToArray();

            return similarUsers;
        }

        public BookScore[] PredictScoreForAllUsersBooks(List<UsersSimilarity> similarUsers, int userId)
        {
            var booksUserRead = _common.GetAllBooksUserReadWithScores(userId);

            var predictedScores = new BookScore[booksUserRead.Length];
            var meanRateForUser = _common.GetAverageRateForUser(userId) ?? 0;

            // we want to predict scores even for books that user already rated
            similarUsers = CombineUniqueAndMutualBooksForUser(similarUsers);
            for (var i = 0; i < booksUserRead.Length; i++)
            {
                var book = booksUserRead[i];
                predictedScores[i] = EvaluateScore(similarUsers, book, meanRateForUser);
            }
            
            return predictedScores;
        }

        private BookScore EvaluateScore(IEnumerable<UsersSimilarity> similarUsers, BookScore bookScore,
            double meanRateForUser)
        {
            var scoreNumerator = 0.0;
            var scoreDenominator = 0.0;
            var rate = 0.0;
           
                foreach (var u in similarUsers)
                {
                    var book = u.BooksUniqueForComparedUser.FirstOrDefault(x => x.BookId == bookScore.BookId);

                    if (book == null || u.AverageScoreForComparedUser == null || u.Similarity == null) continue;
                    scoreNumerator += (book.Rate - u.AverageScoreForComparedUser.Value) * u.Similarity.Value;
                    scoreDenominator += Math.Abs(u.Similarity.Value);
                }

                if (scoreDenominator != 0)
                {
                    rate = scoreNumerator / scoreDenominator + meanRateForUser;
                }

                bookScore.PredictedRate = Math.Round(rate, 2);
                return bookScore;
            
        }
    }
}