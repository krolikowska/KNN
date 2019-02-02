using System.Collections.Generic;
using DataAccess;

namespace RecommendationEngine
{
    public interface IUsersSelector
    {
        List<int> SelectUsersIdsToCompareWith(int userId);
        List<int> GetUsersWhoRatedAtLeastNBooks();
        List<UsersSimilarity> GetSimilarUsersFromDb(int userId, int settingsVersion);
        int[] GetListOfUsersWithComputedSimilarityForGivenSettings(int settingId);
        List<int> GetMostActiveUsersWhoReadMostPopularBooks(int numberOfUsers, int noOfBooksUsersAtLeastRead);
    }
}