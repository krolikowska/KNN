﻿using RecommendationApi.Models;
using RecommendationEngine;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;

namespace RecommendationApi.Controllers
{
    /// <summary>
    ///     A controller for handling recommendations.
    /// </summary>
    public class RecommendationsController : ApiController
    {
        private readonly IUserBasedCollaborativeFiltering _runner;
        private readonly ISettings _settings;
        private readonly IRecommendationEvaluator _evaluator;

        public RecommendationsController(IUserBasedCollaborativeFiltering runner, ISettings settings, IRecommendationEvaluator evaluator)
        {
            _runner = runner;
            _settings = settings;
            _evaluator = evaluator;
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
                var booksForUser = _runner.RecommendBooksForUser(userId, _settings);

                if (booksForUser.Length == 0)
                {
                    return Content<IEnumerable<BookModel>>(HttpStatusCode.NoContent, null);
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

        [HttpGet]
        [Route("EvaluateScore/{userId}")]
        [SwaggerOperation("Evaluate score for books user read",
            OperationId = "EvaluateScore")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.ServiceUnavailable)]
        public double EvaluateScore(int userId)
        {
            return _evaluator.EvaluateScoreForUSer(userId, _settings);
        }
    }
}