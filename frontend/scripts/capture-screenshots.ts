import { chromium } from "@playwright/test";
import { mkdir } from "node:fs/promises";
import path from "node:path";

const BASE_URL = process.env.SCREENSHOT_BASE_URL ?? "http://localhost:3000";

const OUTPUT_DIR = path.join(__dirname, "..", "..", "docs", "images");

const TIMEOUT_MS = 120_000;

const PREPARE_FOR_CAPTURE = `
  *, *::before, *::after {
    animation-duration: 0s !important;
    animation-delay: 0s !important;
    transition-duration: 0s !important;
    transition-delay: 0s !important;
  }

  nextjs-portal {
    display: none !important;
  }

  html {
    scrollbar-gutter: auto !important;
  }
`;

async function main() {
  await mkdir(OUTPUT_DIR, { recursive: true });

  const browser = await chromium.launch({ args: ["--hide-scrollbars"] });
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    deviceScaleFactor: 2,
    reducedMotion: "reduce",
    colorScheme: "light",
  });

  const page = await context.newPage();
  page.setDefaultTimeout(TIMEOUT_MS);
  page.setDefaultNavigationTimeout(TIMEOUT_MS);

  async function settle() {
    await page.waitForLoadState("networkidle");
    await page.waitForFunction(
      () => document.querySelectorAll('[data-slot="skeleton"]').length === 0,
    );
    await page.waitForFunction(() =>
      Array.from(document.images).every((img) => img.complete),
    );
    await page.evaluate(() => document.fonts.ready.then(() => true));
    await page.addStyleTag({ content: PREPARE_FOR_CAPTURE });
  }

  async function shoot(name: string) {
    await settle();
    const file = path.join(OUTPUT_DIR, `${name}.webp`);
    await page.screenshot({
      path: file,
      type: "webp",
      quality: 90,
      fullPage: true,
    });
    console.log(`Saved ${file}`);
  }

  await page.goto(BASE_URL);
  await shoot("storefront");

  await page.goto(`${BASE_URL}/shop`);
  await shoot("catalogue");

  await page.locator('a[href^="/albums/"]').first().click();
  await page.waitForURL(/\/albums\//);
  await shoot("album-detail");

  await page.goto(`${BASE_URL}/shop`);
  await settle();
  await page.getByRole("button", { name: "Add to cart" }).nth(0).click();
  await page.getByRole("button", { name: "Add to cart" }).nth(1).click();
  await page.getByRole("link", { name: "Cart (2)" }).waitFor();

  await page.goto(`${BASE_URL}/cart`);
  await page.getByRole("heading", { name: "Your cart" }).waitFor();
  await shoot("cart");

  await page.getByRole("link", { name: "Checkout" }).click();
  await page.waitForURL((url) => url.pathname === "/checkout");
  await page.getByRole("link", { name: "Log in" }).click();
  await page.waitForURL((url) => url.pathname === "/login");
  await settle();
  await page.getByRole("button", { name: "Try the demo account" }).click();
  await page.waitForURL((url) => url.pathname === "/checkout");
  await page.getByRole("button", { name: /^Place order/ }).waitFor();
  await shoot("checkout");

  await browser.close();
}

main().catch((error: unknown) => {
  console.error(error);
  process.exit(1);
});
