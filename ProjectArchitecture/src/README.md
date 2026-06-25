# Architecture diagram sources

The diagrams in [`../`](..) (`big-picture.png`, `catalog.png`, …) are generated from
the HTML/CSS sources in this folder. They are not hand-drawn images — edit a source
and re-render to update the PNG.

## Layout

```
src/
  build/         # one <name>.html per diagram + shared style.css
  logos/         # brand logos (SVG/PNG) referenced by the HTML as ../logos/*
  render-all.sh  # renders every build/<name>.html -> ../<name>.png
```

## How rendering works

Each `build/<name>.html` is a self-contained styled page. `render-all.sh` opens it in a
headless **Chromium/Edge** browser and captures a 2x `--screenshot`. No Node/Bun toolchain
is needed — only a Chromium-based browser (Edge or Chrome).

## Regenerate

```bash
# from this folder; auto-detects Edge/Chrome on Windows
bash render-all.sh
# or point it at a specific browser
EDGE="/path/to/chrome" bash render-all.sh
```

## Edit a diagram

1. Open `build/<name>.html`, tweak the markup (boxes, labels, logos).
2. Shared styling lives in `build/style.css`.
3. Re-run `render-all.sh` (adjust the per-diagram `width height` in the script if a
   diagram grows and gets clipped or leaves extra whitespace).

Logos are sourced from [devicon](https://devicon.dev), [simple-icons](https://simpleicons.org),
and the .NET package icons (MediatR / Mapster / FluentValidation / Carter).
