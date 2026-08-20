import { describe, expect, test, vi } from "vitest";
import { getToken } from "next-auth/jwt";
import type { JWT } from "@auth/core/jwt";

import { requireAccessToken } from "./require-access-token";

vi.mock("next-auth/jwt", () => ({
  getToken: vi.fn(),
}));

const mockGetToken = vi.mocked(getToken);

function fixtureToken(overrides: Partial<JWT> = {}): JWT {
  return {
    id: "11111111-1111-1111-1111-111111111111",
    firstName: "Fixture",
    lastName: "User",
    email: "fixture@example.test",
    accessToken: "fixture-access-token",
    refreshToken: "fixture-refresh-token",
    expiresAt: Date.now() + 60_000,
    ...overrides,
  };
}

const request = new Request("https://example.test/api/cart");

describe("requireAccessToken", () => {
  test("returns the access token when the JWT decodes cleanly", async () => {
    mockGetToken.mockResolvedValue(fixtureToken());

    await expect(requireAccessToken(request)).resolves.toBe(
      "fixture-access-token",
    );
  });

  test("returns null when there is no token", async () => {
    mockGetToken.mockResolvedValue(null);

    await expect(requireAccessToken(request)).resolves.toBeNull();
  });

  test("returns null when the token carries a refresh error", async () => {
    mockGetToken.mockResolvedValue(fixtureToken({ error: "RefreshError" }));

    await expect(requireAccessToken(request)).resolves.toBeNull();
  });
});
