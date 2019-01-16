using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using RecommendationEngine.Properties;

namespace RecommendationEngine
{
    /// <summary>
    ///     A nearest neighbors search.
    /// </summary>
    public class NearestNeighborsSearch : INearestNeighborsSearch
    {
        /// <summary>
        ///     <see cref="Type" /> of the distance.
        /// </summary>
        private readonly DistanceSimilarityEnum _distanceType;

        /// <summary>
        ///     The neighbors.
        /// </summary>
        private readonly int _kNeighbors;

        /// <summary>
        ///     The minimum number of users who rated book.
        /// </summary>
        private readonly int _minNumOfUsersWhoRatedBook;

        /// <summary>
        ///     The common.
        /// </summary>
        private readonly ICommon _common;

        /// <summary>
        ///     Options for controlling the operation.
        /// </summary>
        private readonly ISettings _settings;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="settings">
        ///     Options for controlling the operation.
        /// </param>
        /// <param name="common">The common.</param>
        public NearestNeighborsSearch(ISettings settings, ICommon common)
        {
            _common = common;
            _settings = settings;
            _distanceType = _settings.SimilarityDistance;
            _kNeighbors = _settings.NumOfNeighbors;
            _minNumOfUsersWhoRatedBook = _settings.BookPopularityAmongUsers;
        }

        /// <summary>
        ///     Calculates the similarity between users.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <param name="comparedUserId">
        ///     Identifier for the compared user.
        /// </param>
        /// <returns>
        ///     The calculated similarity between users.
        /// </returns>
        public UsersSimilarity ComputeSimilarityBetweenUsers(int userId, int comparedUserId)
        {
            var similarity = _common.GetMutualAndUniqueBooks(userId, comparedUserId);
          //  var similarity = _common.IdentifyUniqueAndMutualBooksForUsers(userId, comparedUserId);
            
            if (similarity == null) return null;

            
            switch (_distanceType)
            {
                case DistanceSimilarityEnum.CosineSimilarity:
                    similarity.Similarity = GetCosineDistance(similarity.UserRatesForMutualBooks,
                                                              similarity.ComparedUserRatesForMutualBooks);

                    break;

                case DistanceSimilarityEnum.PearsonSimilarity:
                    similarity.Similarity =
                        GetPearsonCorrelationSimilarity(similarity.UserRatesForMutualBooks,
                                                        similarity.ComparedUserRatesForMutualBooks);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            similarity.SimilarityType = (int) _distanceType;
            return similarity;
        }

        /// <summary>
        ///     Gets nearest neighbors.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <returns>
        ///     The nearest neighbors.
        /// </returns>
        public List<UsersSimilarity> GetNearestNeighbors(int userId)
        {
            var usersToCompare = _common.SelectUsersIdsToCompareWith(userId);
            var neighbors = GetNearestNeighbors(userId, usersToCompare);
            _common.PersistSimilarUsersInDb(neighbors, userId);
            return neighbors;
        }

        public List<UsersSimilarity> GetNearestNeighborsFromDb(int userId)
        {
            return _common.GetSimilarUsersFromDb(userId);
        }

        /// <summary>
        ///     Gets nearest neighbors.
        /// </summary>
        /// <param name="userId">Identifier for the user.</param>
        /// <param name="usersToCompare">The users to compare.</param>
        /// <returns>
        ///     The nearest neighbors.
        /// </returns>
        public List<UsersSimilarity> GetNearestNeighbors(int userId, List<int> usersToCompare)
        {
            var comparer = DetermineComparerFromDistanceType();
            Console.WriteLine($"get for {userId} with {usersToCompare.Count} users to compare");
            UsersSimilarity similarity;
            var sortedList = new SortedSet<UsersSimilarity>(comparer);
            var _lock = new object();

         //   var opt = new ParallelOptions {MaxDegreeOfParallelism = 4};
            Parallel.ForEach(usersToCompare,
                             comparedUser =>
                             {
                                 
                                 lock (_lock)
                                 {
                                     similarity = ComputeSimilarityBetweenUsers(userId, comparedUser);

                                     if (similarity?.Similarity != null) sortedList.Add(similarity);
                                 }
                             });

            //foreach (var u in usersToCompare)
            //{
            //    if (u != null)
            //    {
            //        similarity = ComputeSimilarityBetweenUsers(userId, u.UserId);

            //        if (similarity?.Similarity != null) sortedList.Add(similarity);
            //    }
            //}

            return sortedList.Count == 0 ? null : sortedList.Take(_kNeighbors).ToList();
        }

        /// <summary>
        ///     Determine comparer from distance type.
        /// </summary>
        /// <returns>
        ///     An IComparer<UsersSimilarity>
        /// </returns>
        public IComparer<UsersSimilarity> DetermineComparerFromDistanceType()
        {
            if (_distanceType == DistanceSimilarityEnum.PearsonSimilarity) return new UsersSimilarityReverseComparer();

            return new UsersSimilarityComparer();
        }

        /// <summary>
        ///     Gets cosine distance.
        /// </summary>
        /// <param name="user1rates">The user 1rates.</param>
        /// <param name="user2rates">The user 2rates.</param>
        /// <returns>
        ///     The cosine distance.
        /// </returns>
        public double GetCosineDistance(BookScore[] user1rates, BookScore[] user2rates)
        {
            var dotProduct = 0.0;
            var l2norm1 = 0.0;
            var l2norm2 = 0.0;

            for (var i = 0; i < user1rates.Length; i++)
            {
                dotProduct += user1rates[i].Rate * user2rates[i].Rate;
                l2norm1 += user1rates[i].Rate * user1rates[i].Rate;
                l2norm2 += user2rates[i].Rate * user2rates[i].Rate;
            }

            return dotProduct / (Math.Sqrt(l2norm1) * Math.Sqrt(l2norm2));
        }

        /// <summary>
        ///     Gets Pearson correlation similarity.
        /// </summary>
        /// <param name="user1rates">The user 1rates.</param>
        /// <param name="user2rates">The user 2rates.</param>
        /// <returns>
        ///     The pearson correlation similarity.
        /// </returns>
        public double GetPearsonCorrelationSimilarity(BookScore[] user1rates, BookScore[] user2rates)
        {
            var rAvg1 = user1rates.Average(x => x.Rate);
            var rAvg2 = user2rates.Average(x => x.Rate);
            var dotProduct = 0.0;
            var l2norm1 = 0.0;
            var l2norm2 = 0.0;

            for (var i = 0; i < user1rates.Length; i++)
            {
                dotProduct += (user1rates[i].Rate - rAvg1) * (user2rates[i].Rate - rAvg2);
                l2norm1 += (user1rates[i].Rate - rAvg1) * (user1rates[i].Rate - rAvg1);
                l2norm2 += (user2rates[i].Rate - rAvg2) * (user2rates[i].Rate - rAvg2);
            }

            var denominator = Math.Sqrt(l2norm1) * Math.Sqrt(l2norm2);

            if (Math.Abs(denominator) > 0.000001) return dotProduct / denominator;

            return 0;
        }
    }
}