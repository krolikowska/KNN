using DataAccess;
using System.Collections.Generic;

namespace RecommendationEngine
{
    public interface ICommon
    {
        double? GetAverageRateForUser(int userId);

        List<UsersSimilarity> GetSimilarUsersFromDb(int userID);

        string[] GetUniqueBooksIds(List<UsersSimilarity> similarUsers);

        UsersSimilarity IdentifyUniqueAndMutualBooksForUsers(int userId, int comparedUserID);

        UsersSimilarity IdentifyUniqueAndMutualBooksForUsers(UserSimilar userSimilarFromDb);

        void PersistRecommendedBooksInDb(BookScore[] books, int userId);

        void PersistSimilarUsersInDb(List<UsersSimilarity> neighbors, int userId);

        string[] PreparePotentionalBooksToRecommendation(List<UsersSimilarity> similarUsers);

        List<User> SelectUsersToCompareWith(int userId);

        List<User> SelectUsersToCompareWith(int userId, int[] usersToCompareIds);
    }
}