using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DataAccess;
using RecommendationEngine.Properties;
using CsvHelper;

namespace RecommendationEngine
{
    public class Common : ICommon
    {
        private readonly int _minNumberOfBooksUserRated;
        private readonly int _minNumOfUsersWhoRatedBook;
        private readonly int _bookPopularity;
        private readonly int _settingsVersion;

        private readonly IDataManager _context;

        public Common(ISettings settings, IDataManager dataManager)
        {
            _context = dataManager;
            _minNumberOfBooksUserRated = settings.MinNumberOfBooksEachUserRated;
            _minNumOfUsersWhoRatedBook = settings.MinNumberOfBooksEachUserRated;
            _bookPopularity = settings.BookPopularityAmongUsers;
            _settingsVersion = settings.Id;
        }

        public string[] GetUniqueBooksIds(List<UsersSimilarity> similarUsers)
        {
            // unique list of books we recommend
            return similarUsers
                   .SelectMany(x => x.BooksUniqueForComparedUser
                                     .Select(b => b.BookId))
                   .Distinct()
                   .ToArray();
        }

        public UsersSimilarity GetMutualAndUniqueBooks(UserSimilar userSimilarFromDb)
        {
            var similarity =
                GetMutualAndUniqueBooks(userSimilarFromDb.UserId, userSimilarFromDb.NeighborId);
            similarity.Similarity = userSimilarFromDb.Similarity;

            return similarity;
        }

        public UsersSimilarity GetMutualAndUniqueBooks(int userId, int comparedUserId)
        {
            var books1 = _context.GetBooksRatesByUserId(userId);
            var books2 = _context.GetBooksRatesByUserId(comparedUserId);
            var comparer = new BookScoreEqualityComparer();

       
            var userRatesForMutualBooks = books1.Intersect(books2, comparer).ToArray();
            if (userRatesForMutualBooks.Length == 0) return null;
            var comparedUserRatesForMutualBooks = books2.Intersect(books1, comparer).ToArray();

            var uniqueBooksForComparedUser = books2.Except(books1, comparer).ToArray();
            
            var similarity = new UsersSimilarity
            {
                BooksUniqueForComparedUser = uniqueBooksForComparedUser,
                UserRatesForMutualBooks = userRatesForMutualBooks,
                ComparedUserRatesForMutualBooks = comparedUserRatesForMutualBooks,
                UserId = userId,
                ComparedUserId = comparedUserId,
                AverageScoreForComparedUser = _context.GetAverageRateForUser(comparedUserId),
            };
            return similarity;
        }

        public List<int> SelectUsersIdsToCompareWith(int userId)
        {
            var usersToCompare = _context.GetUsersIdsWithNorMoreRatedBooks(_minNumberOfBooksUserRated);
            if (usersToCompare.Count == 0) return null;

            usersToCompare.Remove(userId); //we won't compare with itself
            return usersToCompare;
        }

        public int[] GetUsersWhoRatedAtLeastNBooks()
        {
           return _context.GetUsersIdsWithNorMoreRatedBooks(_minNumberOfBooksUserRated).ToArray();
        }

        public List<UsersSimilarity> GetSimilarUsersFromDb(int userId, int settingsVersion)
        {
            var similarUsersFromDb = _context.GetUsersNeighbors(userId, settingsVersion);
            var similarUsers = new List<UsersSimilarity>();

            foreach (var similarity in similarUsersFromDb)
            {
                var temp = GetMutualAndUniqueBooks(similarity);
                if (temp != null)
                    similarUsers.Add(temp);
            }

            return similarUsers;
        }

        public int[] GetListOfUsersWithComputedSimilarityForGivenSettings(int settingId) => _context.GetUsersWithComputedSimilarity(_settingsVersion);

        public void PersistRecommendedBooksInDb(BookScore[] books, int userId) => _context.AddRecommendedBooksForUser(books, userId);

        public void PersistTestResults(List<BookScore[]> books, int settingsId) => _context.AddTestResults(books, settingsId);

        public void PersistSimilarUsersInDb(List<List<UsersSimilarity>> neighbors) => _context.AddSimilarUsers(neighbors, _settingsVersion);

        public string[] PreparePotentialBooksToRecommendation(List<UsersSimilarity> similarUsers, int userId)
        {
            var booksIds = GetUniqueBooksIds(similarUsers); // we get list contains all books read by neighbors,
            return _minNumOfUsersWhoRatedBook == 0
                ? booksIds
                : _context.GetBooksIdsRatedByAtLeastNUsers(booksIds,
                                                           _minNumOfUsersWhoRatedBook);
        }

        public double? GetAverageRateForUser(int userId)
        {
            return _context.GetAverageRateForUser(userId);
        }

        public BookScore[] GetAllBooksUserReadWithScores(int userId)
        {
            return _context.GetBooksRatesByUserId(userId);
        }

        public int[] GetUsersWhoReadMostPopularBooks(int numberOfUsers)
        {
            var mostPopularBooks = _context.GetBooksIdsRatedByAtLeastNUsers(numberOfUsers)
                                           .Take(_bookPopularity)
                                           .ToArray();
            return _context.GetUsersWhoRatedAnyOfGivenBooks(mostPopularBooks);
        }

        public void SaveTimesInCsvFile(List<Tuple<int, long>> elapsedTimes, string path)
        {
            using (var writer = new StreamWriter(@"..\..\elapsedTimes.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(elapsedTimes);
            }
        }

        
    }
}