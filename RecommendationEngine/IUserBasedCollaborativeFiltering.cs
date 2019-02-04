using DataAccess;

namespace RecommendationEngine
{
    public interface IUserBasedCollaborativeFiltering
    {
        Book[] RecommendBooksForUser(int userId);
    }
}