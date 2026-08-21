# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Self-directed English learners studying vocabulary for a purpose (e.g. TOEIC prep — the seeded test category is "多益(600)"). Primary job: memorize word meanings and recall them under quiz conditions, repeatedly over time until retained. No account/membership system yet, so today's user is anonymous; the account system (see Capabilities) will attach persistent per-user progress to this same job later.

## Product Purpose

Nooka is a flashcard-style vocabulary learning app. Content is organized into categories ("books," e.g. "多益(600)") each holding many words (term, Chinese meaning, English definition, part of speech, example sentences). Users browse categories like a bookshelf, pick a book, then practice its words through one of several quiz modes. Success means the user can recall a word's meaning reliably, and — once the membership/SM-2 system ships — that the app schedules future review at the right interval so the word sticks long-term.

## Positioning

Not a static flashcard deck: the durable design point is that a correct or incorrect answer feeds a real spaced-repetition schedule (SM-2), not a simple "studied / not studied" toggle. Getting an answer right still doesn't mean "done" — it means the next review is farther out. This makes wrong answers a first-class learning moment (see the answer-detail requirement below), not just a score decrement.

## Operating Context

Flow today: `/practice` (choose a book/category, real data from `GET /api/categories`) → `/practice/[categoryId]` (full word list for that book, `GET /api/words/category/{categoryId}`, plus mode buttons) → pick "選擇題" (multiple choice), choose a practice direction in a modal (看英文選中文 / 看中文選英文) → `/practice/[categoryId]/choice?direction=...` runs the quiz itself. "消消樂" (matching) and "打字拼寫" (typing/spelling) modes are planned but not built (buttons disabled). No login/membership yet; the whole practice flow is currently open to anyone. Backend is ASP.NET Core (.NET 10) + Supabase Postgres via EF Core; frontend is Nuxt 4 with Tailwind v4 + Nuxt UI.

## Capabilities and Constraints

- Multiple-choice quiz: always 4 options (1 correct + 3 random distractors drawn from the same category, which is guaranteed to have >4 words). Direction is chosen before entering the quiz and carried via URL query, not re-selectable mid-quiz.
- **Wrong-answer detail requirement (confirmed, not yet built):** on an incorrect answer, show a detail view with the user's picked (wrong) option's full content on top and the correct answer's full content below — each including definition, part of speech, and example sentences, not just the term. Both `QuizOption` and `QuizQuestion` already carry the full `Word` object for this reason.
- Session-only state: no backend persistence of quiz results yet. No SM-2 scheduling wired up yet — that lands with the membership system (Phase 2 dependency). Do not design as if results are saved.
- No state-management library (Pinia etc.) — quiz state is plain `ref`s in the page component; this is a deliberate constraint for this slice, not an oversight.
- No account/login/roles yet, so no per-user personalization or gating exists on this surface today.
- Real seed data exists for exactly one category today (multiple more will be added by hand in Supabase over time), so content volume in any given book is realistically "one screen's worth to a few dozen words," not thousands.

## Brand Commitments

Name: **Nooka** (nav wordmark rendered with a registered-trademark-style `®` superscript). An established visual identity already exists in code and is binding — this is not a greenfield brand:

- **Night Palette** (the palette for all non-hero interior pages): near-black navy background (`night-bg` `#081428`), slightly lighter panel navy (`night-panel` `#12294a`), warm gold accent (`night-accent` `#f2b84b`), warm off-white foreground (`night-fg` `#f4efe4`), desaturated blue-gray muted text (`night-muted` `#8fa3c2`).
- **Typography**: `Instrument Serif` for display/headings, `Inter` for body — an editorial serif-for-display / clean-sans-for-body pairing, already in use across every existing page.
- **Liquid-glass material language**: translucent, blurred, hairline-bordered "glass" panels/buttons (`.liquid-glass`, `.liquid-glass-cta`, `.liquid-glass-hover` utility classes) are the standard surface treatment for cards, buttons, and modals throughout the app — not a one-off effect.
- Separate **Hero palette** exists only for the marketing landing page's video-background section (`hero-bg`/`hero-fg`/`hero-muted`) and is out of scope for interior app screens like the quiz.

## Evidence on Hand

Real seeded content in Supabase, category "多益(600)" (id 1): 6 words — `vicarious`, `handout`, `hysterical`, `hub`, `haltingly`, `hectic` — each with Chinese meaning, English definition, part of speech, and example sentences. Use this as realistic reference content; do not invent vocabulary content beyond what the API returns.

## Product Principles

- MVP-first: ship the smallest complete vertical slice; defer infrastructure (auth, persistence, caching) until the feature it unblocks is actually being built.
- Session-scoped over-engineering is avoided on purpose — no state library, no premature persistence — until the membership/SM-2 system actually lands.
- Being wrong is a learning moment, not just a scoring event: incorrect answers must surface full context (definition, POS, examples), not just "✗ wrong."
- Consistent Night Palette + liquid-glass system across every interior screen; a new screen inherits this system rather than reinventing it.
