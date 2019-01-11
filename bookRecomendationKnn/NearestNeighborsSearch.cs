///-------------------------------------------------------------------------------------------------
// file:	NearestNeighborsSearch.cs
//
// summary:	Implements the nearest neighbors search class
///-------------------------------------------------------------------------------------------------

using DataAccess;
using RecommendationEngine.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecommendationEngine
{
    /// <summary>   A nearest neighbors search. </summary>
    public class NearestNeighborsSearch : INearestNeighborsSearch
    {
        /// <summary>   Options for controlling the operation. </summary>
        private ISettings _settings;

        /// <summary>   The common. </summary>
        private ICommon _common;

        /// <summary>   Type of the distance. </summary>
        private readonly DistanceSimilarityEnum _distanceType;

        /// <summary>   The neighbors. </summary>
        private readonly int _kNeighbors;

        /// <summary>   The minimum number of users who rated book. </summary>
        private readonly int _minNumOfUsersWhoRatedBook;

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Constructor. </summary>
        ///
        /// <param name="settings"> Options for controlling the operation. </param>
        /// <param name="common">   The common. </param>
        ///-------------------------------------------------------------------------------------------------

        public NearestNeighborsSearch(ISettings settings, ICommon common)
        {
            _common = common;
            _settings = settings;
            _distanceType = _settings.SimilarityDistance;
            _kNeighbors = _settings.NumOfNeighbors;
            _minNumOfUsersWhoRatedBook = _settings.BookPopularityAmongUsers;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Calculates the similarity between users. </summary>
        ///
        /// <param name="userId">           Identifier for the user. </param>
        /// <param name="comparedUserID">   Identifier for the compared user. </param>
        ///
        /// <returns>   The calculated similarity between users. </returns>
        ///-------------------------------------------------------------------------------------------------

        public UsersSimilarity ComputeSimilarityBetweenUsers(int userId, int comparedUserID)
        {
            var similarity = _common.IdentifyUniqueAndMutualBooksForUsers(userId, comparedUserID);
            if (similarity == null) return null;

            switch (_distanceType)
            {
                case DistanceSimilarityEnum.CosineSimilarity:
                    similarity.Similarity = GetCosineDistance(similarity.UserRatesForMutualBooks, similarity.ComparedUserRatesForMutualBooks);

                    break;

                case DistanceSimilarityEnum.PearsonSimilarity:
                    similarity.Similarity = GetPearsonCorrelationSimilarity(similarity.UserRatesForMutualBooks, similarity.ComparedUserRatesForMutualBooks);
                    break;

                default:
                    break;
            }

            similarity.SimilarityType = (int)_distanceType;
            return similarity;
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets nearest neighbors. </summary>
        ///
        /// <param name="userId">   Identifier for the user. </param>
        ///
        /// <returns>   The nearest neighbors. </returns>
        ///-------------------------------------------------------------------------------------------------

        public List<UsersSimilarity> GetNearestNeighbors(int userId)
        {
            var usersToCompare = _common.SelectUsersToCompareWith(userId);
            return GetNearestNeighbors(userId, usersToCompare);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets nearest neighbors. </summary>
        ///
        /// <param name="userId">           Identifier for the user. </param>
        /// <param name="usersToCompare">   The users to compare. </param>
        ///
        /// <returns>   The nearest neighbors. </returns>
        ///-------------------------------------------------------------------------------------------------

        public List<UsersSimilarity> GetNearestNeighbors(int userId, List<User> usersToCompare)
        {
            var comparer = DetermineComparerFromDistanceType();

            UsersSimilarity similarity;
            SortedSet<UsersSimilarity> sortedList = new SortedSet<UsersSimilarity>(comparer);

            Parallel.ForEach(usersToCompare,
                (comparedUser) =>
                {
                    similarity = ComputeSimilarityBetweenUsers(userId, comparedUser.UserId);

                    if (similarity?.Similarity != null)
                    {
                        sortedList.Add(similarity);
                    }
                });

            return sortedList.Take(_kNeighbors).ToList();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Determine comparer from distance type. </summary>
        ///
        /// <returns>   An IComparer&lt;UsersSimilarity&gt; </returns>
        ///-------------------------------------------------------------------------------------------------

        public IComparer<UsersSimilarity> DetermineComparerFromDistanceType()
        {
            if (_distanceType == DistanceSimilarityEnum.PearsonSimilarity)
            {
                return new UsersSimilarityReverseComparer();
            }

            return new UsersSimilarityComparer();
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets cosine distance. </summary>
        ///
        /// <param name="user1rates">   The user 1rates. </param>
        /// <param name="user2rates">   The user 2rates. </param>
        ///
        /// <returns>   The cosine distance. </returns>
        ///-------------------------------------------------------------------------------------------------

        public double GetCosineDistance(BookScore[] user1rates, BookScore[] user2rates)
        {
            double dotProduct = 0.0;
            double l2norm1 = 0.0;
            double l2norm2 = 0.0;

            for (int i = 0; i < user1rates.Length; i++)
            {
                dotProduct += user1rates[i].Rate * user2rates[i].Rate;
                l2norm1 += user1rates[i].Rate * user1rates[i].Rate;
                l2norm2 += user2rates[i].Rate * user2rates[i].Rate;
            }
            return dotProduct / (Math.Sqrt(l2norm1) * Math.Sqrt(l2norm2));
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets pearson correlation similarity. </summary>
        ///
        /// <param name="user1rates">   The user 1rates. </param>
        /// <param name="user2rates">   The user 2rates. </param>
        ///
        /// <returns>   The pearson correlation similarity. </returns>
        ///-------------------------------------------------------------------------------------------------

        public double GetPearsonCorrelationSimilarity(BookScore[] user1rates, BookScore[] user2rates)
        {
            var rAvg1 = user1rates.Average(x => x.Rate);
            var rAvg2 = user2rates.Average(x => x.Rate);
            double dotProduct = 0.0;
            double l2norm1 = 0.0;
            double l2norm2 = 0.0;

            for (int i = 0; i < user1rates.Length; i++)
            {
                dotProduct += (user1rates[i].Rate - rAvg1) * (user2rates[i].Rate - rAvg2);
                l2norm1 += (user1rates[i].Rate - rAvg1) * (user1rates[i].Rate - rAvg1);
                l2norm2 += (user2rates[i].Rate - rAvg2) * (user2rates[i].Rate - rAvg2);
            }
            var denominator = (Math.Sqrt(l2norm1) * Math.Sqrt(l2norm2));

            if (denominator != 0)
            {
                return dotProduct / denominator;
            }

            return 0;
        }
    }
}