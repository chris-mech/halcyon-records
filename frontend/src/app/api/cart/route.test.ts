import { describe, expect, test, vi } from "vitest";

import { client } from "@/lib/api/client";
import { requireAccessToken } from "@/lib/auth/require-access-token";
import type { components } from "@/lib/api/schema";

import { GET } from "./route";

vi.mock("@/lib/api/client", () => ({
  client: { GET: vi.fn() },
}));

vi.mock("@/lib/auth/require-access-token", () => ({
  requireAccessToken: vi.fn(),
}));

type CartItem = components["schemas"]["CartItemResponse"];

const cartItem: CartItem = {
  albumSqid: "cart-route-album",
  title: "Cart Route Fixture Album",
  titleSlug: "cart-route-fixture-album",
  imageUrl: null,
  priceInPence: 1999,
  originalPriceInPence: null,
  quantity: 2,
  unitsInStock: 5,
  isInStock: true,
  artists: [
    {
      sqid: "fixture-artist",
      name: "Fixture Artist",
      nameSlug: "fixture-artist",
    },
  ],
};

const request = new Request("https://example.test/api/cart");

describe("GET /api/cart", () => {
  test("returns 401 when there is no authenticated access token", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue(null);

    const response = await GET(request);

    expect(response.status).toBe(401);
    expect(client.GET).not.toHaveBeenCalled();
  });

  test("forwards the bearer token and returns the backend's cart", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue("fixture-access-token");
    vi.mocked(client.GET).mockResolvedValue({
      data: [cartItem],
      error: undefined,
      response: new Response(null, { status: 200 }),
    });

    const response = await GET(request);

    expect(client.GET).toHaveBeenCalledWith("/api/cart", {
      headers: { Authorization: "Bearer fixture-access-token" },
    });
    expect(response.status).toBe(200);
    await expect(response.json()).resolves.toEqual([cartItem]);
  });

  test("passes through the backend's error status", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue("fixture-access-token");
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { detail: "No cart found." },
      response: new Response(null, { status: 404 }),
    });

    const response = await GET(request);

    expect(response.status).toBe(404);
    await expect(response.json()).resolves.toEqual({
      detail: "No cart found.",
    });
  });
});
