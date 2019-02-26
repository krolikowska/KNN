using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Shouldly;

namespace DataAccess.Tests
{
    public class UsersSimilarityReverseComparerTests
    {
        private readonly UsersSimilarityReverseComparer _sut;
        public UsersSimilarityReverseComparerTests()
        {
                _sut = new UsersSimilarityReverseComparer();
        }

        [Theory]
        [InlineData(1, 2, 10, 1.2)]
        [InlineData(1, 20, 10, 1.2)]
        public void Compare_ShouldReturnEqual(int userId, int comparedUserId, int mutualBooksLenth, double similarity)
        {
            var x = new UsersSimilarity
            {
                UserId = userId,
                ComparedUserId = comparedUserId,
                ComparedUserRatesForMutualBooks = new BookScore[mutualBooksLenth],
                Similarity = similarity
            };

            var y = new UsersSimilarity
            {
                UserId = 1,
                ComparedUserId = 2,
                ComparedUserRatesForMutualBooks = new BookScore[10],
                Similarity = 1.2
            };

            _sut.Compare(x, y).ShouldBe(0);
        }

        [Theory]
        [InlineData(1, 2, 10, 2.2)]
        [InlineData(1, 12, 11, 1.2)]
        [InlineData(11, 2, 10, 11.2)]
        [InlineData(1, 2, 5, 11.2)]
        public void Compare_ShouldReturnGreater(int userId, int comparedUserId, int mutualBooksLenth, double similarity)
        {
            var x = new UsersSimilarity
            {
                UserId = userId,
                ComparedUserId = comparedUserId,
                ComparedUserRatesForMutualBooks = new BookScore[mutualBooksLenth],
                Similarity = similarity
            };

            var y = new UsersSimilarity
            {
                UserId = 1,
                ComparedUserId = 2,
                ComparedUserRatesForMutualBooks = new BookScore[10],
                Similarity = 1.2
            };

            _sut.Compare(x, y).ShouldBe(-1);
        }

        [Theory]
        [InlineData(1, 2, 10, 0.2)]
        [InlineData(1, 12, 9, 1.2)]
        [InlineData(11, 2, 10, 0.2)]
        [InlineData(1, 2, 20, 0.2)]
        public void Compare_ShouldReturnLower(int userId, int comparedUserId, int mutualBooksLenth, double similarity)
        {
            var x = new UsersSimilarity
            {
                UserId = userId,
                ComparedUserId = comparedUserId,
                ComparedUserRatesForMutualBooks = new BookScore[mutualBooksLenth],
                Similarity = similarity
            };

            var y = new UsersSimilarity
            {
                UserId = 1,
                ComparedUserId = 2,
                ComparedUserRatesForMutualBooks = new BookScore[10],
                Similarity = 1.2
            };

            _sut.Compare(x, y).ShouldBe(1);
        }
    }
}
