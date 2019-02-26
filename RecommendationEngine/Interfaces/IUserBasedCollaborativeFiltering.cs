using System.Collections.Generic;
using DataAccess;

namespace RecommendationEngine
{
    public interface IUserBasedCollaborativeFiltering
    {
        Book[] RecommendBooksForUser(int userId, ISettings settings);
    }
}