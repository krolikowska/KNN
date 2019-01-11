using System.Collections.Generic;

namespace bookRecomendationKnn
{
    public class BookComparer : IEqualityComparer<Book>
    {
        public bool Equals(Book x, Book y)
        {
            return x.ISBN == y.ISBN &&
                x.BookTitle == y.BookTitle &&
                x.BookAuthor == y.BookAuthor;
        }

        public int GetHashCode(Book obj)
        {
            return obj.ISBN.GetHashCode();
        }
    }

    public class RatesComparer : IEqualityComparer<BookRates>
    {
        public bool Equals(BookRates x, BookRates y)
        {
            return x.BookId == y.BookId;
        }

        public int GetHashCode(BookRates obj)
        {
            return obj.BookId.GetHashCode();
        }
    }
}