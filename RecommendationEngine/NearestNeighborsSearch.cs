using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DataAccess;

namespace RecommendationEngine
{
    public class NearestNeighborsSearch : INearestNeighborsSearch
    {
        private readonly IUsersSelector _usersSelector;

        public NearestNeighborsSearch(IUsersSelector usersSelector)
        {
            _usersSelector = usersSelector;
        }

        private UsersSimilarity ComputeSimilarityBetweenUsers(int userId, int comparedUserId,
            DistanceSimilarityEnum similarityDistance)
        {
            var similarity = _usersSelector.SelectMutualAndUniqueBooksForUsers(userId, comparedUserId);

            if (similarity == null) return null;

            switch (similarityDistance)
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

            similarity.SimilarityType = (int) similarityDistance;
            return similarity;
        }

        public List<UsersSimilarity> GetNearestNeighbors(int userId, List<int> usersToCompare, ISettings settings)
        {
            Console.WriteLine($"get for {userId} with {usersToCompare.Count} users to compare");
            var sortedList = new SortedSet<UsersSimilarity>(new UsersSimilarityReverseComparer());

            foreach (var comparedUser in usersToCompare)
            {
                if (userId == comparedUser) continue;
                var similarity = ComputeSimilarityBetweenUsers(userId, comparedUser, settings.SimilarityDistance);

                if (similarity?.Similarity != null) sortedList.Add(similarity);
            }

            return sortedList.Count == 0 ? null : sortedList.Take(settings.NumOfNeighbors).ToList();
        }

        private double GetCosineDistance(BookScore[] firstUserRates, BookScore[] secondUserRates)
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

        private double GetPearsonCorrelationSimilarity(BookScore[] firstUserRates, BookScore[] secondUserRates)
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