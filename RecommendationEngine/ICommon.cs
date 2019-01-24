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

        UsersSimilarity GetMutualAndUniqueBooks(int userId, int comparedUserId);
        void PersistRecommendedBooksInDb(BookScore[] books, int userId);

        void PersistSimilarUsersInDb(List<List<UsersSimilarity>> neighbors);

        string[] PreparePotentialBooksToRecommendation(List<UsersSimilarity> similarUsers, int userId);


        BookScore[] GetAllBooksUserReadWithScores(int userId);

        void SaveTimesInCsvFile(List<Tuple<int, long>> elapsedTimes, string path);
    }
}