///-------------------------------------------------------------------------------------------------
// file:	UserBasedCollaborativeFiltering.cs
//
// summary:	Implements the user based collaborative filtering class
///-------------------------------------------------------------------------------------------------

using DataAccess;

namespace RecommendationEngine
{
    /// <summary>   A user based collaborative filtering. </summary>
    public class UserBasedCollaborativeFiltering : IUserBasedCollaborativeFiltering
    {
        /// <summary>   The recommender. </summary>
        private IBookRecommender _recommender;

        /// <summary>   The nearest neighbors. </summary>
        private INearestNeighborsSearch _nearestNeighbors;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        ///
        /// <param name="recommender">      The recommender. </param>
        /// <param name="nearestNeighbors"> The nearest neighbors. </param>
        ///-------------------------------------------------------------------------------------------------

        public UserBasedCollaborativeFiltering(IBookRecommender recommender, INearestNeighborsSearch nearestNeighbors)
        {
            _recommender = recommender;
            _nearestNeighbors = nearestNeighbors;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Recommend books for user. </summary>
        ///
        /// <param name="userId">   Identifier for the user. </param>
        ///
        /// <returns>   A BookScore[]. </returns>
        ///-------------------------------------------------------------------------------------------------

        public BookScore[] RecommendBooksForUser(int userId)
        {
            var similarUsers = _nearestNeighbors.GetNearestNeighbors(userId);

            return _recommender.GetRecommendedBooks(similarUsers, userId);
        }
    }
}