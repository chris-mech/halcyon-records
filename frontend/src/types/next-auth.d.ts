export {};

declare module "@auth/core/types" {
  interface User {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    accessToken: string;
    refreshToken: string;
    expiresAt: number;
  }

  interface Session {
    user: {
      id: string;
      firstName: string;
      lastName: string;
      email: string;
    };
    error?: "RefreshError";
  }
}

declare module "@auth/core/jwt" {
  interface JWT {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    accessToken: string;
    refreshToken: string;
    expiresAt: number;
    error?: "RefreshError";
  }
}
