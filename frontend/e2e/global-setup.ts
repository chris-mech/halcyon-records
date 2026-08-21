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
];

async function globalSetup(): Promise<void> {
  const context = await request.newContext({
    baseURL: "http://localhost:3000",
  });

  for (const route of WARM_UP_ROUTES) {
    try {
      await context.get(route, { timeout: 60_000 });
    } catch {}
  }

  await context.dispose();
}

export default globalSetup;
