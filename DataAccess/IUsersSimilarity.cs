namespace DataAccess {
    public interface IUsersSimilarity {
        int UserId { get; set; }
        int ComparedUserId { get; set; }
        double? Similarity { get; set; }
        BookScore[] ComparedUserRatesForMutualBooks { get; set; }
        BookScore[] UserRatesForMutualBooks { get; set; }
        BookScore[] BooksUniqueForComparedUser { get; set; }
        double? AverageScoreForComparedUser { get; set; }
        int SimilarityType { get; set; }
    }
}