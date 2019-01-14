using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DataAccess;
using RecommendationAPI.Models;
using RecommendationEngine;

namespace RecommendationAPI.Controllers
{
    /// <summary>
    ///     A controller for handling recommendations.
    /// </summary>
    public class RecommendationsController : ApiController
    {
        private readonly IDataManager _dm;
        private readonly IUserBasedCollaborativeFiltering _runner;

        public RecommendationsController(IDataManager dm, IUserBasedCollaborativeFiltering runner)
        {
            _dm = dm;
            _runner = runner;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>   GET api/<controller>/5. </summary>
        /// <param name="userId">   Identifier for the user. </param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the recommended book identifiers
        ///     for users in this collection.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet]
        [Route("recommendedBooks/{userId}")]
        public IEnumerable<BookModel> GetRecommendedBookIdsForUser(int userId)
        {
            return _dm.GetRecommendedBooksForUser(userId)
                      .Select(x => new BookModel
                      {
                          BookAuthor = x.BookAuthor,
                          BookTitle = x.BookTitle,
                          ImageURLL = x.ImageURLL,
                          ISBN = x.ISBN,
                          ImageURLM = x.ImageURLM,
                          ImageURLS = x.ImageURLS,
                          Publisher = x.Publisher,
                          YearOfPublication = x.YearOfPublication
                      });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>   POST api/<controller>/VALUE. </summary>
        /// <param name="userId">   Identifier for the user. </param>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("invokeAlghoritm/{userId}")]
        public void Post(int userId)
        {
            _runner.RecommendBooksForUser(userId);
        }
    }
}