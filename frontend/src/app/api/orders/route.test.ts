import { describe, expect, test, vi } from "vitest";

import { client } from "@/lib/api/client";
import { requireAccessToken } from "@/lib/auth/require-access-token";

import { GET, POST } from "./route";

vi.mock("@/lib/api/client", () => ({
  client: { GET: vi.fn(), POST: vi.fn() },
}));

vi.mock("@/lib/auth/require-access-token", () => ({
  requireAccessToken: vi.fn(),
}));

function ordersRequest(body: unknown) {
  return new Request("https://example.test/api/orders", {
    method: "POST",
    body: JSON.stringify(body),
  });
}

const validBody = {
  contactFirstName: "Order",
  contactLastName: "Contact",
  contactEmail: "order-contact@test.invalid",
  idempotencyKey: "fixture-idempotency-key",
};

describe("GET /api/orders", () => {
  test("returns 401 when there is no authenticated access token", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue(null);

    const response = await GET(new Request("https://example.test/api/orders"));

    expect(response.status).toBe(401);
    expect(client.GET).not.toHaveBeenCalled();
  });

  test("forwards the bearer token and page params, returning the backend's paged orders", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue("fixture-access-token");
    const paged = {
      items: [],
      page: 2,
      pageSize: 5,
      totalCount: 0,
      totalPages: 0,
    };
    vi.mocked(client.GET).mockResolvedValue({
      data: paged,
      error: undefined,
      response: new Response(null, { status: 200 }),
    });

    const response = await GET(
      new Request("https://example.test/api/orders?page=2&pageSize=5"),
    );

    expect(client.GET).toHaveBeenCalledWith("/api/orders", {
      headers: { Authorization: "Bearer fixture-access-token" },
      params: { query: { page: 2, pageSize: 5 } },
    });
    expect(response.status).toBe(200);
    await expect(response.json()).resolves.toEqual(paged);
  });
});

describe("POST /api/orders", () => {
  test("returns 401 when there is no authenticated access token", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue(null);

    const response = await POST(ordersRequest(validBody));

    expect(response.status).toBe(401);
    expect(client.POST).not.toHaveBeenCalled();
  });

  test("returns 400 when the request body is not valid JSON", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue("fixture-access-token");

    const response = await POST(
      new Request("https://example.test/api/orders", {
        method: "POST",
        body: "not json",
      }),
    );

    expect(response.status).toBe(400);
    expect(client.POST).not.toHaveBeenCalled();
  });

  test("forwards the bearer token and body, returning the backend's created order", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue("fixture-access-token");
    const order = {
      orderNumber: "ORD-000001",
      placedAt: "2026-08-20T00:00:00Z",
      totalInPence: 1500,
      items: [],
    };
    vi.mocked(client.POST).mockResolvedValue({
      data: order,
      error: undefined,
      response: new Response(null, { status: 201 }),
    });

    const response = await POST(ordersRequest(validBody));

    expect(client.POST).toHaveBeenCalledWith("/api/orders", {
      headers: { Authorization: "Bearer fixture-access-token" },
      body: validBody,
    });
    expect(response.status).toBe(201);
    await expect(response.json()).resolves.toEqual(order);
  });

  test("passes through the backend's validation error", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue("fixture-access-token");
    vi.mocked(client.POST).mockResolvedValue({
      data: undefined,
      error: { title: "Validation failed." },
      response: new Response(null, { status: 400 }),
    });

    const response = await POST(ordersRequest(validBody));

    expect(response.status).toBe(400);
    await expect(response.json()).resolves.toEqual({
      title: "Validation failed.",
    });
  });
});
