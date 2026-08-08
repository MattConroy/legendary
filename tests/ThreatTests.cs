using Legendary.Companion.Models;
using Xunit;
using static Legendary.Companion.Tests.Content;

namespace Legendary.Companion.Tests;

public class ThreatTests
{
    public class Band
    {
        [Theory]
        [InlineData(1, DifficultyBand.Easy)]
        [InlineData(3, DifficultyBand.Easy)]
        [InlineData(4, DifficultyBand.Medium)]
        [InlineData(7, DifficultyBand.Medium)]
        [InlineData(8, DifficultyBand.Hard)]
        [InlineData(10, DifficultyBand.Hard)]
        public void Uses_the_agreed_thresholds(int score, DifficultyBand band)
            => Assert.Equal(band, new Threat(score).Band);
    }

    public class From
    {
        [Theory]
        [InlineData(1, 1, 1)]    // Red Skull + easy scheme -> floor
        [InlineData(1, 3, 1)]    // base 1, no nudge
        [InlineData(3, 3, 5)]    // medium mastermind, neutral scheme
        [InlineData(3, 5, 6)]    // scheme nudges +1
        [InlineData(5, 3, 9)]    // brutal mastermind, neutral scheme
        [InlineData(5, 5, 10)]   // Thanos + brutal scheme -> ceiling
        [InlineData(4, 1, 6)]    // hard mastermind, easy scheme nudges -1
        public void Adds_a_small_scheme_modifier_to_the_mastermind_base(int mm, int scheme, int expected)
        {
            var m = new Mastermind { Id = "m", Name = "M", Difficulty = mm };
            var s = new Scheme { Id = "s", Name = "S", Difficulty = scheme };
            Assert.Equal(expected, Threat.From(m.ThreatBase!.Value, s.ThreatModifier).Score);
        }

        [Fact]
        public void Reaches_every_difficulty_band_across_the_full_catalogue()
        {
            // Guards the calibration: some mastermind+scheme pair must land in each band,
            // or a target (esp. Easy) could never be honoured.
            var masterminds = Sets.SelectMany(s => s.Masterminds).ToList();
            var schemes = Sets.SelectMany(s => s.Schemes).ToList();
            var bands = (from m in masterminds
                         from s in schemes
                         select Threat.From(m.ThreatBase!.Value, s.ThreatModifier).Band)
                        .Distinct().ToHashSet();
            Assert.Contains(DifficultyBand.Easy, bands);
            Assert.Contains(DifficultyBand.Medium, bands);
            Assert.Contains(DifficultyBand.Hard, bands);
        }
    }
}
