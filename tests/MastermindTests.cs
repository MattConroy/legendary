using Legendary.Companion.Models;
using Xunit;

namespace Legendary.Companion.Tests;

public class MastermindTests
{
    public class ThreatBase
    {
        [Theory]
        [InlineData(1, 1)]   // rating 1 -> base 1
        [InlineData(2, 3)]
        [InlineData(3, 5)]
        [InlineData(4, 7)]
        [InlineData(5, 9)]   // rating 5 -> base 9
        public void Is_the_rating_times_two_minus_one(int rating, int expected)
            => Assert.Equal(expected, new Mastermind { Id = "m", Name = "M", Difficulty = rating }.ThreatBase);

        [Fact]
        public void Is_null_when_the_mastermind_is_unrated()
            => Assert.Null(new Mastermind { Id = "m", Name = "M" }.ThreatBase);
    }
}
