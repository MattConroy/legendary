using Legendary.Companion.Models;
using Xunit;

namespace Legendary.Companion.Tests;

public class KeywordTests
{
    public class HasFullRules
    {
        [Fact]
        public void Is_true_when_the_rules_add_detail_beyond_the_summary()
        {
            var k = new Keyword
            {
                Id = "k", Name = "K", Summary = "Short reminder.",
                Rules = ["A materially longer rules paragraph that clearly exceeds the summary length."],
            };
            Assert.True(k.HasFullRules);
        }

        [Fact]
        public void Is_false_when_there_are_no_rules()
            => Assert.False(new Keyword { Id = "k", Name = "K", Summary = "Short reminder." }.HasFullRules);

        [Fact]
        public void Is_false_when_the_rules_barely_exceed_the_summary()
        {
            var k = new Keyword { Id = "k", Name = "K", Summary = "Teleport: move it.", Rules = ["Teleport."] };
            Assert.False(k.HasFullRules);
        }
    }
}
