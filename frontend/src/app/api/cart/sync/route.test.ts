import { describe, expect, test, vi } from "vitest";

import { client } from "@/lib/api/client";
import { requireAccessToken } from "@/lib/auth/require-access-token";

import { POST } from "./route";

vi.mock("@/lib/api/client", () => ({
  client: { POST: vi.fn() },
}));

vi.mock("@/lib/auth/require-access-token", () => ({
  requireAccessToken: vi.fn(),
}));

function syncRequest(body: unknown) {
  return new Request("https://example.test/api/cart/sync", {
    method: "POST",
    body: JSON.stringify(body),
  });
}

describe("POST /api/cart/sync", () => {
  test("returns 401 when there is no authenticated access token", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue(null);

    const response = await POST(syncRequest({ items: [] }));

    expect(response.status).toBe(401);
    expect(client.POST).not.toHaveBeenCalled();
  });

  test("returns 400 when the request body is not valid JSON", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue("fixture-access-token");

    const response = await POST(
      new Request("https://example.test/api/cart/sync", {
        method: "POST",
        body: "not json",
      }),
    );

    expect(response.status).toBe(400);
    expect(client.POST).not.toHaveBeenCalled();
  });

  test("forwards the bearer token and body, returning the backend's 204", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue("fixture-access-token");
    vi.mocked(client.POST).mockResolvedValue({
      data: undefined,
      error: undefined,
      response: new Response(null, { status: 204 }),
    });

    const items = [{ albumSqid: "cart-route-album", quantity: 2 }];
    const response = await POST(syncRequest({ items }));

    expect(client.POST).toHaveBeenCalledWith("/api/cart/sync", {
      headers: { Authorization: "Bearer fixture-access-token" },
      body: { items },
    });
    expect(response.status).toBe(204);
  });

  test("passes through the backend's validation error", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue("fixture-access-token");
    vi.mocked(client.POST).mockResolvedValue({
      data: undefined,
      error: { title: "Validation failed." },
      response: new Response(null, { status: 400 }),
    });

    const response = await POST(syncRequest({ items: [] }));

    expect(response.status).toBe(400);
    await expect(response.json()).resolves.toEqual({
      title: "Validation failed.",
    });
  });
});
