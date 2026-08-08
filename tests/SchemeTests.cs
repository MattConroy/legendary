using Legendary.Companion.Models;
using Xunit;

namespace Legendary.Companion.Tests;

public class SchemeTests
{
    public class ThreatModifier
    {
        [Theory]
        [InlineData(1, -1)]  // below average -> nudge down
        [InlineData(2, -1)]
        [InlineData(3, 0)]   // average -> no nudge
        [InlineData(4, 1)]
        [InlineData(5, 1)]   // above average -> nudge up (clamped)
        public void Is_the_rating_minus_three_clamped_to_plus_or_minus_one(int rating, int expected)
            => Assert.Equal(expected, new Scheme { Id = "s", Name = "S", Difficulty = rating }.ThreatModifier);

        [Fact]
        public void Is_null_when_the_scheme_is_unrated()
            => Assert.Null(new Scheme { Id = "s", Name = "S" }.ThreatModifier);
    }
}
