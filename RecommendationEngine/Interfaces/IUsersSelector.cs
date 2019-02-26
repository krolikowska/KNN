using System.Collections.Generic;
using DataAccess;

namespace RecommendationEngine
{
    public interface IUsersSelector
    {
        List<int> SelectUsersIdsToCompareWith(int userId);
        UsersSimilarity SelectMutualAndUniqueBooksForUsers(int userId, int comparedUserId);
    }
}