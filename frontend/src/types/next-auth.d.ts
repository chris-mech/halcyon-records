import type { DefaultSession } from "next-auth";

declare module "next-auth" {
  interface User {
    id: string;
    firstName: string;
    lastName: string;
    accessToken: string;
    refreshToken: string;
    expiresAt: number;
  }

  interface Session {
    user: {
      id: string;
      firstName: string;
      lastName: string;
    } & DefaultSession["user"];
    error?: "RefreshError";
  }
}

declare module "@auth/core/jwt" {
  interface JWT {
    id: string;
    firstName: string;
    lastName: string;
    accessToken: string;
    refreshToken: string;
    expiresAt: number;
    error?: "RefreshError";
  }
}
