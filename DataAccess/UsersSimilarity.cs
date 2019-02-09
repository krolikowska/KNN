using System.Linq;

namespace DataAccess
{
    public class UsersSimilarity
    {
        public int UserId { get; set; }

        public int ComparedUserId { get; set; }

        public double? Similarity { get; set; }

        public BookScore[] ComparedUserRatesForMutualBooks { get; set; }

        public BookScore[] UserRatesForMutualBooks { get; set; }

        public BookScore[] BooksUniqueForComparedUser { get; set; }

        public double? AverageScoreForComparedUser { get; set; }

        public int SimilarityType { get; set; }
    }
}