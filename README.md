# Legendary — Setup Randomiser

A mobile-first **companion app** for the board game *Legendary: A Marvel Deck Building Game*.
One tap generates a complete, rules-legal game setup — **Mastermind, Scheme, Villain Groups,
Henchmen, and Heroes** — for **1–5 players**.

Built as a **Blazor WebAssembly PWA** targeting **.NET 10**, deployable as a static site to
GitHub Pages. Works offline once loaded.

> ⚠️ **Unofficial fan project.** Not affiliated with, endorsed by, or licensed by the game's
> publishers or Marvel. It contains **no card text or art** — only the names needed to randomise a
> legal setup, plus the public setup rules. You must own the physical game to play.

## Features

- **One-tap randomiser** that follows the official per-player setup table (heroes / villain groups /
  henchmen / master strikes / bystanders / wounds).
- **Per-category rerolls** — reroll just the Mastermind, Scheme, Villains, Henchmen, or Heroes while
  everything else stays put.
- **Required groups honoured** — each Mastermind's *"Always Leads"* group is forced into the setup and
  flagged with a **Required** badge. Rerolls re-honour it automatically.
- **Scheme setup modifiers** — Schemes that change counts (e.g. *Replace Earth's Leaders with Killbots*
  adds a Henchman group; *Super Hero Civil War* adds a Hero) are applied automatically.
- **Expansion-ready** — sets are self-contained data modules toggled on/off in **Sets**. The
  randomiser only ever reads the *enabled* pool, so adding an expansion never touches randomiser code.
- **Dark, Marvel-flavoured, mobile-first** UI with a bottom nav, big randomise button, and result cards.
- **Installable PWA** with offline support.

## Project structure

```
src/
  Models/          Card & setup domain types (GameCard, Scheme, SetupRule, GameSetup…)
  Data/            Content modules — CoreSet.cs, ExampleExpansion.cs — and SetRegistry
  Services/        CardPool, SetupRandomizer (set-agnostic), GameStateService (state + storage)
  Components/      ResultCard.razor
  Pages/           Home (randomiser), Settings (sets/options), About (disclaimer + rules)
  Layout/          App shell + bottom navigation
tests/             xUnit tests for the randomiser (counts, required groups, rerolls, toggles)
.github/workflows/ deploy.yml — build + deploy to GitHub Pages
```

## Run locally

```bash
dotnet run --project src        # serves at https://localhost:xxxx
dotnet test  tests              # run the randomiser test suite
```

## Adding an expansion set

No randomiser changes needed:

1. Copy `src/Data/ExampleExpansion.cs` to e.g. `DarkCity.cs`.
2. Fill in the set's Masterminds (with `AlwaysLeadsGroupId`), Schemes (with any `SchemeSetup`
   deltas/notes), Villain Groups, Henchmen, and Heroes. Set `IsExample = false`.
3. Register it in `src/Data/SetRegistry.cs` (`AllSets`).

It now appears as a toggle under **Sets** and is drawn from whenever enabled.

## Deploy to GitHub Pages

This repo ships a workflow at `.github/workflows/deploy.yml` that:

- restores, runs the tests, and `dotnet publish`es the app;
- rewrites `<base href>` **and** the service-worker base to the `/<repo>/` subpath (project sites
  live at `https://<owner>.github.io/<repo>/`);
- adds `.nojekyll` (so `_framework/` isn't stripped) and a `404.html` SPA fallback;
- uploads the result and deploys it via the official Pages actions.

It runs on every push to `main` (and can be run manually from the **Actions** tab via
*Run workflow*).

### Enable Pages (one-time, in the GitHub web UI)

1. Push this repo to GitHub (see the branch note below).
2. Go to your repository → **Settings** (top tab).
3. In the left sidebar, click **Pages**.
4. Under **Build and deployment → Source**, select **GitHub Actions** (not "Deploy from a branch").
5. That's it — there's nothing else to save. Now trigger a deploy:
   - merge/push to `main`, **or**
   - open the **Actions** tab → **Deploy to GitHub Pages** → **Run workflow**.
6. When the run finishes, your site is live at **`https://<owner>.github.io/<repo>/`**
   (for this repo: `https://mattconroy.github.io/legendary/`). The live URL is also shown on the
   workflow run's **deploy** job.

> The workflow derives the subpath from the repo name automatically, so if you rename or fork the
> repo the base href stays correct — no code changes required.
