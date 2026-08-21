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

function meRequest() {
  return new Request("https://example.test/api/auth/me");
}

describe("GET /api/auth/me", () => {
  test("returns 401 when there is no authenticated access token", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue(null);

    const response = await GET(meRequest());

    expect(response.status).toBe(401);
    expect(client.GET).not.toHaveBeenCalled();
  });

  test("forwards the bearer token, returning the backend's current-user response", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue("fixture-access-token");
    const user = {
      id: "11111111-1111-1111-1111-111111111111",
      email: "current-user@test.invalid",
      firstName: "Current",
      lastName: "User",
      registeredAt: "2026-06-14T00:00:00Z",
    };
    vi.mocked(client.GET).mockResolvedValue({
      data: user,
      error: undefined,
      response: new Response(null, { status: 200 }),
    });

    const response = await GET(meRequest());

    expect(client.GET).toHaveBeenCalledWith("/api/auth/me", {
      headers: { Authorization: "Bearer fixture-access-token" },
    });
    expect(response.status).toBe(200);
    await expect(response.json()).resolves.toEqual(user);
  });

  test("passes through a backend error", async () => {
    vi.mocked(requireAccessToken).mockResolvedValue("fixture-access-token");
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { detail: "Something went wrong." },
      response: new Response(null, { status: 500 }),
    });

    const response = await GET(meRequest());

    expect(response.status).toBe(500);
    await expect(response.json()).resolves.toEqual({
      detail: "Something went wrong.",
    });
  });
});
