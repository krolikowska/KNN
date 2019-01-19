using System;
using System.Collections.Generic;
using DataAccess;

namespace RecommendationEngine
{
    public interface ICommon
    {
        List<int> SelectUsersIdsToCompareWith(int userId);

        void PersistTestResults(List<BookScore[]> books, int settingsId);
        int[] GetUsersWhoRatedAtLeastNBooks();
        int[] GetListOfUsersWithComputedSimilarityForGivenSettings(int settingId);
        int[] GetUsersWhoReadMostPopularBooks(int numOfBooks);
        double? GetAverageRateForUser(int userId);

        List<UsersSimilarity> GetSimilarUsersFromDb(int userId, int settingsVersion);

        string[] GetUniqueBooksIds(List<UsersSimilarity> similarUsers);

    //    UsersSimilarity IdentifyUniqueAndMutualBooksForUsers(int userId, int comparedUserId);

      //  UsersSimilarity IdentifyUniqueAndMutualBooksForUsers(UserSimilar userSimilarFromDb);

        UsersSimilarity GetMutualAndUniqueBooks(int userId, int comparedUserId);
        void PersistRecommendedBooksInDb(BookScore[] books, int userId);

        void PersistSimilarUsersInDb(List<UsersSimilarity> neighbors, int userId);

        string[] PreparePotentialBooksToRecommendation(List<UsersSimilarity> similarUsers, int userId);

      //  List<User> SelectUsersToCompareWith(int userId);

    //    List<User> SelectUsersToCompareWith(int userId, int[] usersToCompareIds);

        BookScore[] GetAllBooksUserReadWithScores(int userId);

        void SaveTimesInCsvFile(List<Tuple<int, long>> elapsedTimes, string path);
    }
}