// file:	UsersSimilarity.cs
//
// summary:	Implements the users similarity class

namespace DataAccess
{
    /// <summary>The users similarity.</summary>
    public class UsersSimilarity
    {
        /// <summary>Gets or sets the identifier of the user.</summary>
        /// <value>The identifier of the user.</value>
        public int UserId { get; set; }

        /// <summary>Gets or sets the identifier of the compared user.</summary>
        /// <value>The identifier of the compared user.</value>
        public int ComparedUserId { get; set; }

        /// <summary>Gets or sets the similarity.</summary>
        /// <value>The similarity.</value>
        public double? Similarity { get; set; }

        /// <summary>Gets or sets the compared user rates for mutual books.</summary>
        /// <value>The compared user rates for mutual books.</value>
        public BookScore[] ComparedUserRatesForMutualBooks { get; set; }

        /// <summary>Gets or sets the user rates for mutual books.</summary>
        /// <value>The user rates for mutual books.</value>
        public BookScore[] UserRatesForMutualBooks { get; set; }

        /// <summary>Gets or sets the books unique for compared user.</summary>
        /// <value>The books unique for compared user.</value>
        public BookScore[] BooksUniqueForComparedUser { get; set; }

        /// <summary>Gets or sets the average score for compared user.</summary>
        /// <value>The average score for compared user.</value>
        public double? AverageScoreForComparedUser { get; set; }
        
        /// <summary>Gets or sets the type of the similarity.</summary>
        /// <value>The type of the similarity.</value>
        public int SimilarityType { get; set; }
    }
}