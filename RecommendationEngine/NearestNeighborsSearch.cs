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
    
    public class NearestNeighborsSearch : INearestNeighborsSearch
    {
        private readonly DistanceSimilarityEnum _distanceType;
private readonly int _kNeighbors;
        private readonly int _minNumOfUsersWhoRatedBook;
        private readonly ICommon _common;
        private readonly ISettings _settings;
        public NearestNeighborsSearch(ISettings settings, ICommon common)
        {
            _common = common;
            _settings = settings;
            _distanceType = _settings.SimilarityDistance;
            _kNeighbors = _settings.NumOfNeighbors;
            _minNumOfUsersWhoRatedBook = _settings.BookPopularityAmongUsers;
        }

        public UsersSimilarity ComputeSimilarityBetweenUsers(int userId, int comparedUserId)
        {
            var similarity = _common.GetMutualAndUniqueBooks(userId, comparedUserId);
            
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
        public List<UsersSimilarity> GetNearestNeighbors(int userId)
        {
            var usersToCompare = _common.SelectUsersIdsToCompareWith(userId);
            var neighbors = GetNearestNeighbors(userId, usersToCompare);
            
            return neighbors;
        }

        public List<UsersSimilarity> GetNearestNeighborsFromDb(int userId, int settingsVersion)
        {
            return _common.GetSimilarUsersFromDb(userId, settingsVersion);
        }

        public List<UsersSimilarity> GetNearestNeighbors(int userId, List<int> usersToCompare)
        {
            var comparer = DetermineComparerFromDistanceType();
            Console.WriteLine($"get for {userId} with {usersToCompare.Count} users to compare");
            UsersSimilarity similarity;
            var sortedList = new SortedSet<UsersSimilarity>(comparer);
            var _lock = new object();

            Parallel.ForEach(usersToCompare,
                             comparedUser =>
                             {
                                 
                                 lock (_lock)
                                 {
                                     similarity = ComputeSimilarityBetweenUsers(userId, comparedUser);

                                     if (similarity?.Similarity != null) sortedList.Add(similarity);
                                 }
                             });


            return sortedList.Count == 0 ? null : sortedList.Take(_kNeighbors).ToList();
        }

        public IComparer<UsersSimilarity> DetermineComparerFromDistanceType()
        {
            if (_distanceType == DistanceSimilarityEnum.PearsonSimilarity) return new UsersSimilarityReverseComparer();

            return new UsersSimilarityComparer();
        }

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