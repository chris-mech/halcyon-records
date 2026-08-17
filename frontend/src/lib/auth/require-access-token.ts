import { getToken } from "next-auth/jwt";

async function requireAccessToken(request: Request): Promise<string | null> {
  const token = await getToken({
    req: request,
    secret: process.env.AUTH_SECRET,
  });

  if (!token || token.error === "RefreshError") {
    return null;
  }

  return token.accessToken;
}

export { requireAccessToken };
