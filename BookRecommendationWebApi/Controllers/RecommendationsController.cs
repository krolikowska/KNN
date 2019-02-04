using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using DataAccess;
using RecommendationApi.Models;
using RecommendationEngine;
using RecommendationEngine.Properties;
using Swashbuckle.Swagger.Annotations;

namespace RecommendationApi.Controllers
{
    /// <summary>
    ///     A controller for handling recommendations.
    /// </summary>
    public class RecommendationsController : ApiController
    {
        private readonly IUserBasedCollaborativeFiltering _runner;
        private readonly ISettings _settings;
        private readonly IDataManager _context;

        public RecommendationsController(IUserBasedCollaborativeFiltering runner, ISettings settings,
            IDataManager context)
        {
            _runner = runner;
            _settings = settings;
            _context = context;
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
        [SwaggerOperation("Get recommended books for user by given user id",
            OperationId = "GetRecommendedBookIdsForUser")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<BookModel>))]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.ServiceUnavailable)]
        public NegotiatedContentResult<IEnumerable<BookModel>> GetRecommendedBookIdsForUser(int userId)
        {
            try
            {
                //by default get from db
                var booksForUser = _context.GetRecommendedBooksForUser(userId).ToArray();
                if (booksForUser.Length == 0)
                {
                    // if not in db, run algorithm
                    booksForUser = _runner.RecommendBooksForUser(userId);

                    if (booksForUser.Length == 0)
                    {
                        return Content<IEnumerable<BookModel>>(HttpStatusCode.NoContent, null);
                    }
                }

                var books = booksForUser
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

                return Content(HttpStatusCode.OK, books);
            }
            catch
            {
                return Content<IEnumerable<BookModel>>(HttpStatusCode.ServiceUnavailable, null);
            }
        }

        ///// -------------------------------------------------------------------------------------------------
        ///// <summary>   POST api/<controller>/VALUE. </summary>
        ///// <param name="userId">   Identifier for the user. </param>
        ///// -------------------------------------------------------------------------------------------------
        //[HttpGet]
        //[Route("recommendedBooks/{userId}")]
        //[SwaggerOperation("Get recommended books for user by given user id",
        //    OperationId = "GetRecommendedBookIdsForUser")]
        //[SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<BookModel>))]
        //[SwaggerResponse(HttpStatusCode.NoContent)]
        //[SwaggerResponse(HttpStatusCode.ServiceUnavailable)]
        //public void Post(int userId)
        //{
        //    _runner.RecommendBooksForUser(userId);
        //}
    }
}