using DataAccess;

namespace RecommendationEngine
{
    public interface IUserBasedCollaborativeFiltering
    {
        BookScore[] RecommendBooksForUser(int userId);
    }
}