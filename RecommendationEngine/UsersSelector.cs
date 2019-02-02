﻿using System.Collections.Generic;
using System.Linq;
using DataAccess;
using RecommendationEngine.Properties;

namespace RecommendationEngine
{
    public class UsersSelector : IUsersSelector
    {
        private readonly int _bookPopularity;
        private readonly int _minNumberOfBooksUserRated;
        private readonly int _settingsVersion;

        private readonly IDataManager _context;

        public UsersSelector(IDataManager context, ISettings settings)
        {
            _context = context;
            _bookPopularity = settings.BookPopularityAmongUsers;
            _minNumberOfBooksUserRated = settings.MinNumberOfBooksEachUserRated;
            _settingsVersion = settings.Id;
        }

        public List<int> SelectUsersIdsToCompareWith(int userId)
        {
            var usersToCompare = _context.GetUserIdsWithNorMoreRatedBooks(_minNumberOfBooksUserRated);
            if (usersToCompare.Count == 0) return null;

            usersToCompare.Remove(userId); //we won't compare with itself
            return usersToCompare;
        }

        public List<int> GetUsersWhoRatedAtLeastNBooks()
        {
            return _context.GetUserIdsWithNorMoreRatedBooks(_minNumberOfBooksUserRated);
        }

        public List<UsersSimilarity> GetSimilarUsersFromDb(int userId, int settingsVersion)
        {
            var similarUsersFromDb = _context.GetUserNeighbors(userId, settingsVersion);
            var similarUsers = new List<UsersSimilarity>();

            foreach (var similarity in similarUsersFromDb)
            {
                var temp = GetMutualAndUniqueBooks(similarity);
                if (temp != null)
                    similarUsers.Add(temp);
            }

            return similarUsers;
        }

        public int[] GetListOfUsersWithComputedSimilarityForGivenSettings(int settingId) =>
            _context.GetAllUsersWithComputedSimilarity(_settingsVersion);

        public List<int> GetMostActiveUsersWhoReadMostPopularBooks(int booksReadByAtLeastNoOfUsers,
            int noOfBooksUsersAtLeastRead)
        {
            var mostPopularBooks = _context.GetBooksIdsRatedByAtLeastNUsers(booksReadByAtLeastNoOfUsers)
                                           .Take(_bookPopularity)
                                           .ToArray();
            var users = _context.GetUsersWhoRatedAnyOfGivenBooks(mostPopularBooks);
            var mostActiveUsers = _context.GetUserIdsWithNorMoreRatedBooks(noOfBooksUsersAtLeastRead);
            return users.Intersect(mostActiveUsers).ToList();
        }

        public UsersSimilarity GetMutualAndUniqueBooks(UserSimilar userSimilarFromDb)
        {
            var similarity = UsersSimilarity.GetMutualAndUniqueBooks(userSimilarFromDb.UserId, userSimilarFromDb.NeighborId);
            if (similarity == null) return null;
            similarity.Similarity = userSimilarFromDb.Similarity;

            return similarity;
        }
    }
}