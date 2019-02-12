using DataAccess;

namespace RecommendationEngine
{
    public class UserBasedCollaborativeFiltering : IUserBasedCollaborativeFiltering
    {
        private readonly INearestNeighborsSearch _nearestNeighbors;
        private readonly IBookRecommender _recommender;
        private readonly IUsersSelector _selector;
        private readonly CollaborativeFilteringHelpers _helpers;

        public UserBasedCollaborativeFiltering(
            IBookRecommender recommender,
            INearestNeighborsSearch nearestNeighbors,
            CollaborativeFilteringHelpers helpers,
            IUsersSelector selector)
        {
            _recommender = recommender;
            _nearestNeighbors = nearestNeighbors;
            _helpers = helpers;
            _selector = selector;
        }

        public Book[] RecommendBooksForUser(int userId, ISettings settings)
        {
            // get from db by default
            var books = _recommender.GetRecommendedBooksFromDatabase(userId);
            if (books.Length != 0)
            {
                return books;
            }

            var users = _selector.SelectUsersIdsToCompareWith(userId);
            var similarUsers = _nearestNeighbors.GetNearestNeighbors(userId, users, settings);

            return _recommender.GetRecommendedBooks(similarUsers, userId);
        }
    }
}