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
- **Scheme setup modifiers** — Schemes that change counts (e.g. *Negative Zone Prison Breakout* adds a
  Henchman group; *Steal the Weaponized Plutonium* adds a Villain group; *Secret Invasion* forces the
  Skrulls) are applied automatically, with the right Twist counts per Scheme.
- **Content sets** — **Core Set**, **Dark City**, and **Fantastic Four**, toggleable under **Sets**.
- **Data-driven** — all sets/cards live in `wwwroot/data/sets.json`, loaded at runtime; the randomiser
  only reads the *enabled* pool, so adding an expansion is a data change, not a code change.
- **Setup counts surfaced** — Master Strikes shown on the Mastermind card, Scheme Twists on the
  Scheme card, and Bystanders on the Villain Groups card, per the official setup.
- **Dark, Marvel-flavoured, mobile-first** UI with a bottom nav, big randomise button, and result cards.
- **Installable PWA** with offline support and a custom comic-style icon.

## Project structure

```
src/
  Models/            Card & setup domain types with their own behaviour (GameCard, Scheme,
                     SetupRule, GameSetup, Threat…)
  Abstractions/      Repository ports the app depends on (ISetRepository, IKeywordRepository,
                     IPreferenceRepository) — returning domain objects
  Data/              JSON parse+validate (SetJson/KeywordJson) and the repository adapters
                     (HttpSetRepository, HttpKeywordRepository, LocalStoragePreferenceRepository)
  Services/          CardPool, SetupRandomizer (set-agnostic), GameStateService (state)
  Components/        ResultCard, TeamIcon
  Pages/             Home (randomiser), Settings (sets/options), About (disclaimer + rules)
  Layout/            App shell + bottom navigation
  wwwroot/data/      sets.json — the content (all sets/cards), loaded at runtime
  wwwroot/teams/     team badge SVGs
assets/              icon.svg + render-icons.mjs (Chromium-rendered PWA icons)
tests/               xUnit tests for the randomiser + a validation pass over sets.json
.github/workflows/   deploy.yml — build + deploy to GitHub Pages
```

## Run locally

```bash
dotnet run --project src        # serves at https://localhost:xxxx
dotnet test  tests              # run the randomiser test suite
```

## Adding an expansion set

Content lives in **`src/wwwroot/data/sets.json`** and is loaded at runtime — no code or
randomiser changes needed (and, once hosted from a database/API, no redeploy):

1. Add a set object to the JSON array: `id`, `name`, `released`, `standalone`, arrays of
   `masterminds` (each may declare `alwaysLeadsGroupId`), `schemes` (with a `setup` block —
   `twists`, deltas, forced groups, `bystanders`, `notes`), `villainGroups`, `henchmen`, `heroes`.
2. Give every set/card a **stable, unique `id`** — ids are permanent local-storage keys for a
   player's owned/in-play selections, so never rename or reuse one.
3. If the set introduces a **new hero team**, add `wwwroot/teams/<slug>.svg` and map the team name
   in `Components/TeamIcon.razor` (the only change that needs a redeploy).

`SetJson.Validate` (run on load and in tests) checks ids are unique and every "always-leads" /
forced-group reference resolves.

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
