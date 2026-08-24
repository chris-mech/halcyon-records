import { request } from "@playwright/test";

const API_BASE_URL = "https://localhost:7000";

const WARM_UP_ROUTES = [
  "/",
  "/shop",
  "/search",
  "/register",
  "/login",
  "/cart",
  "/checkout",
  "/checkout/confirmation",
  "/account",
  "/account/orders/warm-up-placeholder",
  "/artists",
  "/genres",
  "/decades",
  "/account/details",
];

async function resetAlbumStock(): Promise<void> {
  const context = await request.newContext({
    baseURL: API_BASE_URL,
    ignoreHTTPSErrors: true,
  });

  try {
    const response = await context.post("/api/dev/albums/restock");
    console.log(`[stock-reset] -> ${response.status()}`);
  } catch (error) {
    console.log(`[stock-reset] -> failed: ${error}`);
  } finally {
    await context.dispose();
  }
}

async function globalSetup(): Promise<void> {
  await resetAlbumStock();

  const context = await request.newContext({
    baseURL: "http://localhost:3000",
  });

  for (const route of WARM_UP_ROUTES) {
    const start = Date.now();
    try {
      const response = await context.get(route, { timeout: 60_000 });
      console.log(
        `[warm-up] ${route} -> ${response.status()} (${Date.now() - start}ms)`,
      );
    } catch (error) {
      console.log(
        `[warm-up] ${route} -> failed after ${Date.now() - start}ms: ${error}`,
      );
    }
  }

  await context.dispose();
}

export default globalSetup;
