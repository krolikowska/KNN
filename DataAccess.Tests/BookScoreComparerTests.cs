using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace DataAccess.Tests
{
    public class BookScoreComparerTests
    {
        private readonly BookScoreComparer _sut;

        public BookScoreComparerTests()
        {
            _sut = new BookScoreComparer();
        }

        [Theory]
        [InlineData(1, "1", 5)]
        [InlineData(1, "3", 5)]
        public void BookScoreComparer_ShouldReturnEqual(int userId, string bookId, double predictedRate)
        {
            var x = new BookScore
            {
                UserId = 1,
                BookId = "1",
                PredictedRate = 5,
                Rate = 23
            };

            var y = new BookScore
            {
                UserId = userId,
                BookId = bookId,
                PredictedRate = predictedRate,
                Rate = 23
            };

            _sut.Compare(x, y).ShouldBe(0);
        }

        [Theory]
        [InlineData(1, "1", 10)]
        [InlineData(1, "2", 15)]
        [InlineData(2, "1", 15)]
        public void BookScoreComparer_ShouldReturnGrater(int userId, string bookId, double predictedRate)
        {
            var x = new BookScore
            {
                UserId = 1,
                BookId = "1",
                PredictedRate = 5,
                Rate = 23
            };

            var y = new BookScore
            {
                UserId = userId,
                BookId = bookId,
                PredictedRate = predictedRate,
                Rate = 23
            };

            _sut.Compare(x, y).ShouldBe(1);
        }

        [Theory]
        [InlineData(1, "1", 1)]
        [InlineData(1, "2", 2)]
        [InlineData(2, "1", 2)]
        public void BookScoreComparer_ShouldReturnLower(int userId, string bookId, double predictedRate)
        {
            var x = new BookScore
            {
                UserId = 1,
                BookId = "1",
                PredictedRate = 5,
                Rate = 23
            };

            var y = new BookScore
            {
                UserId = userId,
                BookId = bookId,
                PredictedRate = predictedRate,
                Rate = 23
            };

            _sut.Compare(x, y).ShouldBe(-1);
        }



    }
}
