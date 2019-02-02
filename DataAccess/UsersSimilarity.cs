using System.Linq;

namespace DataAccess
{
    public class UsersSimilarity : IUsersSimilarity
    {
        public int UserId { get; set; }

        public int ComparedUserId { get; set; }

        public double? Similarity { get; set; }

        public BookScore[] ComparedUserRatesForMutualBooks { get; set; }

        public BookScore[] UserRatesForMutualBooks { get; set; }

        public BookScore[] BooksUniqueForComparedUser { get; set; }

        public double? AverageScoreForComparedUser { get; set; }

        public int SimilarityType { get; set; }

        private static IDataManager _context;

        public UsersSimilarity(IDataManager context)
        {
            _context = context;
        }

        public static UsersSimilarity GetMutualAndUniqueBooks(int userId, int comparedUserId)
        {
            var books1 = _context.GetBooksRatesByUserId(userId);
            var books2 = _context.GetBooksRatesByUserId(comparedUserId);
            var comparer = new BookScoreEqualityComparer();

            var userRatesForMutualBooks = books1.Intersect(books2, comparer).ToArray();
            if (userRatesForMutualBooks.Length == 0) return null;
            var comparedUserRatesForMutualBooks = books2.Intersect(books1, comparer).ToArray();

            var uniqueBooksForComparedUser = books2.Except(books1, comparer).ToArray();

            return new UsersSimilarity(_context)
            {
                BooksUniqueForComparedUser = uniqueBooksForComparedUser,
                UserRatesForMutualBooks = userRatesForMutualBooks,
                ComparedUserRatesForMutualBooks = comparedUserRatesForMutualBooks,
                UserId = userId,
                ComparedUserId = comparedUserId,
                AverageScoreForComparedUser = _context.GetAverageRateForUser(comparedUserId),
            };
        }
    }
}