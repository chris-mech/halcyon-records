import { request } from "@playwright/test";

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

async function globalSetup(): Promise<void> {
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
