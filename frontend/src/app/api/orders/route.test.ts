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
