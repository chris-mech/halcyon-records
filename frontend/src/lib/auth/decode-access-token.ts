export interface AccessTokenClaims {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

export function decodeAccessToken(accessToken: string): AccessTokenClaims {
  const [, payload] = accessToken.split(".");
  const claims = JSON.parse(
    Buffer.from(payload, "base64url").toString("utf-8"),
  ) as Record<string, unknown>;

  const {
    sub: id,
    email,
    given_name: firstName,
    family_name: lastName,
  } = claims;

  if (
    typeof id !== "string" ||
    typeof email !== "string" ||
    typeof firstName !== "string" ||
    typeof lastName !== "string"
  ) {
    throw new Error("Access token payload is missing expected claims.");
  }

  return { id, email, firstName, lastName };
}
