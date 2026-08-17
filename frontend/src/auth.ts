import NextAuth, { CredentialsSignin } from "next-auth";
import Credentials from "next-auth/providers/credentials";

import { client } from "@/lib/api/client";
import { decodeAccessToken } from "@/lib/auth/decode-access-token";

class InvalidCredentialsError extends CredentialsSignin {
  code = "InvalidCredentials";
}

interface RefreshedTokens {
  accessToken: string;
  refreshToken: string;
  expiresAt: number;
}

const pendingRefreshes = new Map<string, Promise<RefreshedTokens>>();

async function refreshAccessToken(
  refreshToken: string,
): Promise<RefreshedTokens> {
  const pending = pendingRefreshes.get(refreshToken);
  if (pending) {
    return pending;
  }

  const refreshPromise = (async (): Promise<RefreshedTokens> => {
    const { data, error } = await client.POST("/api/auth/refresh", {
      body: { refreshToken },
    });

    if (error) {
      throw new Error(error.detail ?? "Failed to refresh access token.");
    }

    return {
      accessToken: data.accessToken,
      refreshToken: data.refreshToken,
      expiresAt: Date.parse(data.expiresAt),
    };
  })();

  pendingRefreshes.set(refreshToken, refreshPromise);
  try {
    return await refreshPromise;
  } finally {
    pendingRefreshes.delete(refreshToken);
  }
}

export const { handlers, auth, signIn, signOut } = NextAuth({
  providers: [
    Credentials({
      credentials: {
        email: {},
        password: {},
      },
      async authorize(credentials) {
        const { email, password } = credentials;

        if (typeof email !== "string" || typeof password !== "string") {
          throw new InvalidCredentialsError();
        }

        const { data, error } = await client.POST("/api/auth/login", {
          body: { email, password },
        });

        if (error) {
          throw new InvalidCredentialsError();
        }

        const claims = decodeAccessToken(data.accessToken);

        return {
          id: claims.id,
          email: claims.email,
          firstName: claims.firstName,
          lastName: claims.lastName,
          accessToken: data.accessToken,
          refreshToken: data.refreshToken,
          expiresAt: Date.parse(data.expiresAt),
        };
      },
    }),
  ],
  callbacks: {
    async jwt({ token, user }) {
      if (user) {
        token.id = user.id;
        token.firstName = user.firstName;
        token.lastName = user.lastName;
        token.accessToken = user.accessToken;
        token.refreshToken = user.refreshToken;
        token.expiresAt = user.expiresAt;
        token.error = undefined;
        return token;
      }

      if (Date.now() < token.expiresAt) {
        return token;
      }

      try {
        const refreshed = await refreshAccessToken(token.refreshToken);
        token.accessToken = refreshed.accessToken;
        token.refreshToken = refreshed.refreshToken;
        token.expiresAt = refreshed.expiresAt;
        token.error = undefined;
      } catch {
        token.error = "RefreshError";
      }

      return token;
    },
    async session({ session, token }) {
      session.user.id = token.id;
      session.user.firstName = token.firstName;
      session.user.lastName = token.lastName;
      session.error = token.error;
      return session;
    },
  },
  events: {
    async signOut(message) {
      if (!("token" in message) || !message.token) {
        return;
      }

      await client.POST("/api/auth/logout", {
        body: { refreshToken: message.token.refreshToken },
      });
    },
  },
});
