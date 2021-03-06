﻿using System.Collections.Generic;

namespace DataAccess
{
   
    public class UsersSimilarityReverseComparer : IComparer<UsersSimilarity>
    {
        public int Compare(UsersSimilarity x, UsersSimilarity y)
        {
            if (x.UserId == y.UserId && x.Similarity == y.Similarity && x.ComparedUserId == y.ComparedUserId) return 0;
            if (x.Similarity == y.Similarity && x.ComparedUserId != y.ComparedUserId)
            {
               if( x.ComparedUserRatesForMutualBooks.Length == y.ComparedUserRatesForMutualBooks.Length)
                return 0;
               if (x.ComparedUserRatesForMutualBooks.Length > y.ComparedUserRatesForMutualBooks.Length)
                   return -1;
               return 1;
            }

            if (x.Similarity > y.Similarity)
                return -1;
            return 1;
        }
    }

    public class BookScoreComparer : IComparer<BookScore>
    {
        public int Compare(BookScore x, BookScore y)
        {
            if (x.PredictedRate == y.PredictedRate) return 0;
          
            if (x.PredictedRate > y.PredictedRate)
                return -1;
            return 1;
        }
    }

    public class BookScoreEqualityComparer : IEqualityComparer<BookScore>
    {
        public bool Equals(BookScore x, BookScore y)
        {
            return x.BookId == y.BookId;
        }

        public int GetHashCode(BookScore obj)
        {
            return obj.BookId.GetHashCode();
        }
    }
}