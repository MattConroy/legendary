// Renders assets/icon.svg to the PWA PNG icons using the pre-installed Chromium.
// Run: NODE_PATH=/opt/node22/lib/node_modules node assets/render-icons.mjs
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';
import { createRequire } from 'node:module';

const require = createRequire('/opt/node22/lib/node_modules/');
const { chromium } = require('playwright');

const here = dirname(fileURLToPath(import.meta.url));
const outDir = join(here, '..', 'src', 'wwwroot');
const svg = readFileSync(join(here, 'icon.svg'), 'utf8');

const exe = '/opt/pw-browsers/chromium-1194/chrome-linux/chrome';
const targets = [
  [512, 'icon-512.png'],
  [192, 'icon-192.png'],
  [32, 'favicon.png'],
];

const browser = await chromium.launch({ executablePath: exe });
try {
  for (const [size, name] of targets) {
    const page = await browser.newPage({ viewport: { width: size, height: size }, deviceScaleFactor: 1 });
    const html = `<!doctype html><html><head><meta charset="utf-8">
      <style>*{margin:0;padding:0}html,body{width:${size}px;height:${size}px;overflow:hidden}
      svg{display:block;width:${size}px;height:${size}px}</style></head>
      <body>${svg}</body></html>`;
    await page.setContent(html, { waitUntil: 'networkidle' });
    await page.screenshot({ path: join(outDir, name), clip: { x: 0, y: 0, width: size, height: size } });
    await page.close();
    console.log(`wrote ${name} (${size}x${size})`);
  }
} finally {
  await browser.close();
}
