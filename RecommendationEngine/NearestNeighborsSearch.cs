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

        public NearestNeighborsSearch(ISettings settings)
        {
            var settings1 = settings;
            _distanceType = settings1.SimilarityDistance;
            _kNeighbors = settings1.NumOfNeighbors;
        }

        public UsersSimilarity ComputeSimilarityBetweenUsers(int userId, int comparedUserId)
        {
            var similarity = UsersSimilarity.GetMutualAndUniqueBooks(userId, comparedUserId);

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

        public List<UsersSimilarity> GetNearestNeighbors(int userId, List<int> usersToCompare)
        {
            var comparer = DetermineComparerFromDistanceType();
            Console.WriteLine($"get for {userId} with {usersToCompare.Count} users to compare");
            var sortedList = new SortedSet<UsersSimilarity>(comparer);

            foreach (var comparedUser in usersToCompare)
            {

                if(userId == comparedUser) continue;
                var similarity = ComputeSimilarityBetweenUsers(userId, comparedUser);

                if (similarity?.Similarity != null) sortedList.Add(similarity);
            }

            return sortedList.Count == 0 ? null : sortedList.Take(_kNeighbors).ToList();
        }

        public IComparer<UsersSimilarity> DetermineComparerFromDistanceType()
        {
            if (_distanceType == DistanceSimilarityEnum.PearsonSimilarity) return new UsersSimilarityReverseComparer();

            return new UsersSimilarityComparer();
        }

        public double GetCosineDistance(BookScore[] firstUserRates, BookScore[] secondUserRates)
        {
            var numerator = 0.0;
            var denominatorL = 0.0;
            var denominatorR = 0.0;

            for (var i = 0; i < firstUserRates.Length; i++)
            {
                numerator += firstUserRates[i].Rate * secondUserRates[i].Rate;
                denominatorL += firstUserRates[i].Rate * firstUserRates[i].Rate;
                denominatorR += secondUserRates[i].Rate * secondUserRates[i].Rate;
            }

            return numerator / (Math.Sqrt(denominatorL) * Math.Sqrt(denominatorR));
        }

        public double GetPearsonCorrelationSimilarity(BookScore[] firstUserRates, BookScore[] secondUserRates)
        {
            var rAvg1 = firstUserRates.Average(x => x.Rate);
            var rAvg2 = secondUserRates.Average(x => x.Rate);
            var numerator = 0.0;
            var denominatorL = 0.0;
            var denominatorR = 0.0;

            for (var i = 0; i < firstUserRates.Length; i++)
            {
                numerator += (firstUserRates[i].Rate - rAvg1) * (secondUserRates[i].Rate - rAvg2);
                denominatorL += (firstUserRates[i].Rate - rAvg1) * (firstUserRates[i].Rate - rAvg1);
                denominatorR += (secondUserRates[i].Rate - rAvg2) * (secondUserRates[i].Rate - rAvg2);
            }

            var denominator = Math.Sqrt(denominatorL) * Math.Sqrt(denominatorR);

            if (Math.Abs(denominator) > 0.000001) return numerator / denominator;

            return 0;
        }
    }
}