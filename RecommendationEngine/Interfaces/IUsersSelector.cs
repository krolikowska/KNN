﻿using System.Collections.Generic;
using DataAccess;

namespace RecommendationEngine
{
    public interface IUsersSelector
    {
        List<int> SelectUsersIdsToCompareWith(int userId);
        List<int> GetUsersWhoRatedAtLeastNBooks(int numOfBooks);
        List<UsersSimilarity> GetSimilarUsersFromDb(int userId, int settingsVersion);     
        UsersSimilarity SelectMutualAndUniqueBooksForUsers(int userId, int comparedUserId);
    }
}