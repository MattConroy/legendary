using Legendary.Companion.Abstractions;
using Legendary.Companion.Models;
using Legendary.Companion.Services;
using Xunit;

namespace Legendary.Companion.Tests;

public class GameStateServiceTests
{
    private static GameStateService NewState(out FakePreferences prefs)
    {
        prefs = new FakePreferences();
        return new GameStateService(new SetupRandomizer(new Random(1)), new FakeSetRepository(), prefs);
    }

    public class SetPlayers
    {
        [Fact]
        public async Task Changes_the_current_count_without_touching_the_saved_default()
        {
            var state = NewState(out var prefs);
            await state.InitializeAsync();

            state.SetPlayers(4);

            Assert.Equal(4, state.Players);          // current game follows the change
            Assert.Equal(2, state.DefaultPlayers);   // saved default is untouched
            Assert.False(prefs.Scalars.ContainsKey("legendary.players")); // nothing persisted
        }

        [Fact]
        public async Task Does_not_survive_a_reload()
        {
            var prefs = new FakePreferences();
            var first = new GameStateService(new SetupRandomizer(new Random(1)), new FakeSetRepository(), prefs);
            await first.InitializeAsync();
            first.SetPlayers(4); // just for tonight's game

            var reloaded = new GameStateService(new SetupRandomizer(new Random(1)), new FakeSetRepository(), prefs);
            await reloaded.InitializeAsync();

            Assert.Equal(2, reloaded.Players); // back to the default, not 4
        }
    }

    public class SetDefaultPlayersAsync
    {
        [Fact]
        public async Task Persists_the_default_and_applies_it_to_the_current_game()
        {
            var state = NewState(out var prefs);
            await state.InitializeAsync();

            await state.SetDefaultPlayersAsync(5);

            Assert.Equal(5, state.DefaultPlayers);
            Assert.Equal(5, state.Players);
            Assert.Equal("5", prefs.Scalars["legendary.players"]);
        }

        [Fact]
        public async Task Is_restored_as_the_default_on_the_next_load()
        {
            var prefs = new FakePreferences();
            var first = new GameStateService(new SetupRandomizer(new Random(1)), new FakeSetRepository(), prefs);
            await first.InitializeAsync();
            await first.SetDefaultPlayersAsync(3);

            var reloaded = new GameStateService(new SetupRandomizer(new Random(1)), new FakeSetRepository(), prefs);
            await reloaded.InitializeAsync();

            Assert.Equal(3, reloaded.DefaultPlayers);
            Assert.Equal(3, reloaded.Players);
        }
    }

    // ----- in-memory fakes for the repository ports -----

    private sealed class FakeSetRepository : ISetRepository
    {
        public IReadOnlyList<CardSet> Sets => Content.Sets;
        public bool IsLoaded => true;
        public Task EnsureLoadedAsync() => Task.CompletedTask;
        public CardSet? FindById(string id) => Sets.FirstOrDefault(s => s.Id == id);
    }

    private sealed class FakePreferences : IPreferenceRepository
    {
        public Dictionary<string, string> Scalars { get; } = [];
        private readonly Dictionary<string, IReadOnlyList<string>> _lists = [];

        public Task<string?> GetAsync(string key) =>
            Task.FromResult(Scalars.TryGetValue(key, out var v) ? v : null);

        public Task SetAsync(string key, string value)
        {
            Scalars[key] = value;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>?> GetListAsync(string key) =>
            Task.FromResult(_lists.TryGetValue(key, out var v) ? v : null);

        public Task SetListAsync(string key, IEnumerable<string> values)
        {
            _lists[key] = values.ToList();
            return Task.CompletedTask;
        }
    }
}
