using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookRecomendationKnn
{
    public class DataManagerHelpers
    {
        public DataManagerHelpers()
        {
            ClearDb();
        }
        public void AddUser(User user)
        {
            using (var context = new BooksRecomendationsEntities())
            {
                context.Users.Add(user);
                context.SaveChanges();
            }
        }

        public void AddBook(Book book)
        {
            using (var context = new BooksRecomendationsEntities())
            {
                context.Books.Add(book);
                context.SaveChanges();
            }
        }

        public void AddBooksRatings(BooksRating rate)
        {
            using (var context = new BooksRecomendationsEntities())
            {
                context.BooksRatings.Add(rate);
                context.SaveChanges();
            }
        }

       
        

        public void ClearDb()
        {
            using (var context = new BooksRecomendationsEntities())
            {
                context.UserSimilars.Clear();
                context.BookRecomendations.Clear();
                context.BooksRatings.Clear();
                context.Books.Clear();
                context.Users.Clear();
                context.SaveChanges();
            }
        }

    }
}
