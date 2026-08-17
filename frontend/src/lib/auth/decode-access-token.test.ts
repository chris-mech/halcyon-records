import { describe, expect, test } from "vitest";

import { decodeAccessToken } from "./decode-access-token";

function fakeAccessToken(claims: Record<string, unknown>): string {
  const header = Buffer.from(JSON.stringify({ alg: "none" })).toString(
    "base64url",
  );
  const payload = Buffer.from(JSON.stringify(claims)).toString("base64url");
  return `${header}.${payload}.`;
}

describe("decodeAccessToken", () => {
  test("extracts id, email, firstName, and lastName from the standard JWT claims", () => {
    const token = fakeAccessToken({
      sub: "11111111-1111-1111-1111-111111111111",
      email: "user@example.com",
      given_name: "Given Name Claim",
      family_name: "Family Name Claim",
    });

    expect(decodeAccessToken(token)).toEqual({
      id: "11111111-1111-1111-1111-111111111111",
      email: "user@example.com",
      firstName: "Given Name Claim",
      lastName: "Family Name Claim",
    });
  });

  test("throws when a required claim is missing", () => {
    const token = fakeAccessToken({ sub: "id-only" });

    expect(() => decodeAccessToken(token)).toThrow();
  });
});
