import { describe, expect, test, vi } from "vitest";

import { client } from "@/lib/api/client";
import { requireAccessToken } from "@/lib/auth/require-access-token";

import { GET } from "./route";

vi.mock("@/lib/api/client", () => ({
  client: { GET: vi.fn() },
}));

vi.mock("@/lib/auth/require-access-token", () => ({
  requireAccessToken: vi.fn(),
}));

function orderDetailRequest() {
  return new Request("https://example.test/api/orders/ORD-000001");
}

function ctxFor(orderNumber: string) {
  return { params: Promise.resolve({ orderNumber }) };
}

describe("GET /api/orders/[orderNumber]", () => {
  test("returns 401 when there is no authenticated access token", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue(null);

    const response = await GET(orderDetailRequest(), ctxFor("ORD-000001"));

    expect(response.status).toBe(401);
    expect(client.GET).not.toHaveBeenCalled();
  });

  test("forwards the bearer token and order number, returning the backend's order detail", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue("fixture-access-token");
    const order = {
      orderNumber: "ORD-000001",
      placedAt: "2026-08-20T00:00:00Z",
      status: "Placed",
      contactFirstName: "Order",
      contactLastName: "Contact",
      contactEmail: "order-contact@test.invalid",
      totalInPence: 1500,
      items: [],
    };
    vi.mocked(client.GET).mockResolvedValue({
      data: order,
      error: undefined,
      response: new Response(null, { status: 200 }),
    });

    const response = await GET(orderDetailRequest(), ctxFor("ORD-000001"));

    expect(client.GET).toHaveBeenCalledWith("/api/orders/{orderNumber}", {
      headers: { Authorization: "Bearer fixture-access-token" },
      params: { path: { orderNumber: "ORD-000001" } },
    });
    expect(response.status).toBe(200);
    await expect(response.json()).resolves.toEqual(order);
  });

  test("passes through the backend's not-found error", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue("fixture-access-token");
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { code: "Order.NotFound", detail: "Order not found." },
      response: new Response(null, { status: 404 }),
    });

    const response = await GET(orderDetailRequest(), ctxFor("ORD-000001"));

    expect(response.status).toBe(404);
    await expect(response.json()).resolves.toEqual({
      code: "Order.NotFound",
      detail: "Order not found.",
    });
  });
});
