using System.Collections.Generic;
using System.Linq;
using DataAccess;

namespace RecommendationEngine
{
    public class UsersSelector : IUsersSelector
    {
        private readonly int _minNumberOfBooksUserRated;
        private readonly int _settingsVersion;

        private readonly IDataManager _context;

        public UsersSelector(IDataManager context, ISettings settings)
        {
            _context = context;
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

        public List<int> GetUsersWhoRatedAtLeastNBooks(int numOfBooks)
        {
            return _context.GetUserIdsWithNorMoreRatedBooks(numOfBooks);
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

    

        public UsersSimilarity GetMutualAndUniqueBooks(UserSimilar userSimilarFromDb)
        {
            var similarity = SelectMutualAndUniqueBooksForUsers(userSimilarFromDb.UserId, userSimilarFromDb.NeighborId);
            if (similarity == null) return null;
            similarity.Similarity = userSimilarFromDb.Similarity;

            return similarity;
        }

        public UsersSimilarity SelectMutualAndUniqueBooksForUsers(int userId, int comparedUserId)
        {
            var books1 = _context.GetBooksRatesByUserId(userId);
            var books2 = _context.GetBooksRatesByUserId(comparedUserId);
            var comparer = new BookScoreEqualityComparer();

            var userRatesForMutualBooks = books1.Intersect(books2, comparer).ToArray();
            if (userRatesForMutualBooks.Length == 0) return null;
            var comparedUserRatesForMutualBooks = books2.Intersect(books1, comparer).ToArray();

            var uniqueBooksForComparedUser = books2.Except(books1, comparer).ToArray();

            return new UsersSimilarity
            {
                BooksUniqueForComparedUser = uniqueBooksForComparedUser,
                UserRatesForMutualBooks = userRatesForMutualBooks,
                ComparedUserRatesForMutualBooks = comparedUserRatesForMutualBooks,
                UserId = userId,
                ComparedUserId = comparedUserId,
                AverageScoreForComparedUser = _context.GetAverageRateForUser(comparedUserId),
            };
        }
    }
}