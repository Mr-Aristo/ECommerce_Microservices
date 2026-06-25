#!/usr/bin/env bash
# Regenerate the architecture diagram PNGs from the HTML sources.
#
# Pipeline: each build/<name>.html (styled with build/style.css + ../logos/*)
# is rendered to a crisp 2x PNG via headless Chromium/Edge --screenshot.
# No Node/Bun required — only a Chromium-based browser.
#
# Usage:   bash render-all.sh            # auto-detects Edge/Chrome on Windows
#          EDGE="/path/to/chrome" bash render-all.sh
#
# Output:  ../<name>.png  (i.e. ProjectArchitecture/<name>.png)
set -u
DIR="$(cd "$(dirname "$0")" && pwd)"
DIRW="$(cd "$(dirname "$0")" && pwd -W 2>/dev/null || echo "$DIR")"   # Windows path for file:// URLs
OUT="$(cd "$DIR/.." && pwd)"

# locate a Chromium-based browser
if [ -z "${EDGE:-}" ]; then
  for c in \
    "/c/Program Files (x86)/Microsoft/Edge/Application/msedge.exe" \
    "/c/Program Files/Microsoft/Edge/Application/msedge.exe" \
    "/c/Program Files/Google/Chrome/Application/chrome.exe" \
    "/c/Program Files (x86)/Google/Chrome/Application/chrome.exe"; do
    [ -f "$c" ] && EDGE="$c" && break
  done
fi
[ -z "${EDGE:-}" ] && { echo "No Chromium/Edge found. Set EDGE=/path/to/browser."; exit 1; }

# name  width  height  (canvas sizes tuned per diagram)
RENDER="
big-picture       1480 720
checkout-flow     1460 560
identity-security 1260 430
api-gateway       1280 480
catalog           1180 700
basket            1240 790
discount          1340 600
ordering          1240 740
users             1200 740
payment           1200 660
notification      1140 600
"
echo "$RENDER" | while read -r n w h; do
  [ -z "$n" ] && continue
  "$EDGE" --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=2 \
    --window-size="$w,$h" --screenshot="$OUT/$n.png" "file:///$DIRW/build/$n.html" 2>/dev/null
  sleep 0.6
  [ -f "$OUT/$n.png" ] && echo "OK  $n.png" || echo "FAIL $n"
done
