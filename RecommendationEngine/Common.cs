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
    /// <summary>
    ///     A common.
    /// </summary>
    public class Common : ICommon
    {
        // ReSharper disable once PrivateMembersMustHaveComments
        private readonly int _minNumberOfBooksUserRated;
        private readonly int _minNumOfUsersWhoRatedBook;
        private readonly int _bookPopularity;
        private readonly int _settingsVersion;

        private readonly IDataManager _context;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="settings">
        ///     Options for controlling the operation.
        /// </param>
        /// <param name="dataManager">Manager for data.</param>
        public Common(ISettings settings, IDataManager dataManager)
        {
            _context = dataManager;
            _minNumberOfBooksUserRated = settings.MinNumberOfBooksEachUserRated;
            _minNumOfUsersWhoRatedBook = settings.MinNumberOfBooksEachUserRated;
            _bookPopularity = settings.BookPopularityAmongUsers;
            _settingsVersion = settings.Id;
        }

        /// <summary>
        ///     Select users to compare with.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <param name="usersToCompareIds">
        ///     <see cref="List`1" /> of identifiers for the users to compares.
        /// </param>
        /// <returns>
        ///     A List<User>
        /// </returns>
        public List<User> SelectUsersToCompareWith(int userId, int[] usersToCompareIds)
        {
            var user = _context.GetUser(userId);
            var usersToCompare = _context.GetUsers(usersToCompareIds);
            if (user == null || usersToCompare == null || usersToCompare.Count == 0) return null;

            usersToCompare.Remove(user); //we won't compare with itself
            return usersToCompare;
        }

        /// <summary>
        ///     Gets unique books identifiers.
        /// </summary>
        /// <param name="similarUsers">The similar users.</param>
        /// <returns>
        ///     An array of string.
        /// </returns>
        public string[] GetUniqueBooksIds(List<UsersSimilarity> similarUsers)
        {
            // unique list of books we recommend
            return similarUsers
                   .SelectMany(x => x.BooksUniqueForComparedUser
                                     .Select(b => b.BookId))
                   .Distinct()
                   .ToArray();
        }

        /// <summary>
        ///     Identify unique and mutual books for users.
        /// </summary>
        /// <param name="userSimilarFromDb">
        ///     The user similar from database.
        /// </param>
        /// <returns>
        ///     An UsersSimilarity.
        /// </returns>
        public UsersSimilarity IdentifyUniqueAndMutualBooksForUsers(UserSimilar userSimilarFromDb)
        {
            var similarity =
                IdentifyUniqueAndMutualBooksForUsers(userSimilarFromDb.UserId, userSimilarFromDb.NeighborId);
            similarity.Similarity = userSimilarFromDb.Similarity;

            return similarity;
        }

        /// <summary>
        ///     Identifief unique and mutual books for users.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <param name="comparedUserId">
        ///     Identifier for the compared user.
        /// </param>
        /// <returns>
        ///     An UsersSimilarity.
        /// </returns>
        public UsersSimilarity IdentifyUniqueAndMutualBooksForUsers(int userId, int comparedUserId)
        {
            var mutualBooksIds = _context.GetBooksIdsSameForBothUsers(userId, comparedUserId);
            if (mutualBooksIds.Length == 0) return null;
            var uniqueBooks = _context.GetBooksIdsUniqueForSecondUser(userId, comparedUserId);


            var similarity = new UsersSimilarity
            {
                BooksUniqueForComparedUser = _context.GetUsersRatesForGivenIsbnList(uniqueBooks, comparedUserId),
                UserRatesForMutualBooks = _context.GetUsersRatesForGivenIsbnList(mutualBooksIds, userId),
                ComparedUserRatesForMutualBooks =
                    _context.GetUsersRatesForGivenIsbnList(mutualBooksIds, comparedUserId),
                UserId = userId,
                ComparedUserId = comparedUserId,
                AverageScoreForComparedUser = _context.GetAverageRateForUser(comparedUserId),
            };
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

        /// <summary>
        ///     Select users to compare with.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     A List<User>
        /// </returns>
        public List<User> SelectUsersToCompareWith(int userId)
        {
            var user = _context.GetUser(userId);
            var usersToCompare = _context.GetUsersWithNorMoreRatedBooks(_minNumberOfBooksUserRated).ToList();
            if (user == null || usersToCompare.Count == 0) return null;

            usersToCompare.Remove(user); //we won't compare with itself
            return usersToCompare;
        }

        public List<int> SelectUsersIdsToCompareWith(int userId)
        {
            var usersToCompare = _context.GetUsersIdsWithNorMoreRatedBooks(_minNumberOfBooksUserRated);
            if (usersToCompare.Count == 0) return null;

            usersToCompare.Remove(userId); //we won't compare with itself
            return usersToCompare;
        }

        public List<User> GetUsersWhoRatedAtLeastNBooks()
        {
           return _context.GetUsersWithNorMoreRatedBooks(_minNumberOfBooksUserRated).ToList();
        }

       

        /// <summary>
        ///     Gets similar users from database.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     The similar users from database.
        /// </returns>
        public List<UsersSimilarity> GetSimilarUsersFromDb(int userId)
        {
            var similarUsersFromDb = _context.GetUsersNeighbors(userId, _settingsVersion);
            var similarUsers = new List<UsersSimilarity>();

            foreach (var similarity in similarUsersFromDb)
            {
                var temp = IdentifyUniqueAndMutualBooksForUsers(similarity);
                if (temp != null)
                    similarUsers.Add(temp);
            }

            return similarUsers;
        }

        /// <summary>
        ///     method for adding neighbors.
        /// </summary>
        /// <param name="books">The books.</param>
        /// <param name="userId">Identifier for the user.</param>
        public void PersistRecommendedBooksInDb(BookScore[] books, int userId)
        {
            _context.AddRecommendedBooksForUser(books, userId);
        }

        /// <summary>
        ///     Persist similar users in database.
        /// </summary>
        /// <param name="neighbors">The neighbors.</param>
        /// <param name="userId">Identifier for the user.</param>
        public void PersistSimilarUsersInDb(List<UsersSimilarity> neighbors, int userId)
        {
            _context.AddSimilarUsers(neighbors, userId, _settingsVersion);
        }

        /// <summary>
        ///     Prepare potential books to recommendation.
        /// </summary>
        /// <param name="similarUsers">The similar users.</param>
        /// <param name="userId"></param>
        /// <returns>
        ///     A string[].
        /// </returns>
        public string[] PreparePotentialBooksToRecommendation(List<UsersSimilarity> similarUsers, int userId)
        {
            var booksIds = GetUniqueBooksIds(similarUsers); // we get list contains all books read by neighbors,
            return _minNumOfUsersWhoRatedBook == 0
                ? booksIds
                : _context.GetBooksIdsRatedByAtLeastNUsers(booksIds,
                                                           _minNumOfUsersWhoRatedBook);
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
            return _context.GetAverageRateForUser(userId);
        }

        /// <summary>
        ///     Gets all books user read with scores.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     An array of book score.
        /// </returns>
        public BookScore[] GetAllBooksUserReadWithScores(int userId)
        {
            return _context.GetBooksRatesByUserId(userId);
        }

        public List<User> GetUsersWhoReadMostPopularBooks(int numberOfUsers)
        {
            var mostPopularBooks = _context.GetBooksIdsRatedByAtLeastNUsers(numberOfUsers)
                                           .Take(_bookPopularity)
                                           .ToArray();
            var users = _context.GetUsersWhoRatedAnyOfGivenBooks(mostPopularBooks);
            return _context.GetUsers(users);
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