import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { useSession } from "next-auth/react";
import type { Session } from "@auth/core/types";

import { auth } from "@/auth";
import { AuthSessionProvider } from "./auth-session-provider";

vi.mock("@/auth", () => ({
  auth: vi.fn(),
}));

const mockAuth = vi.mocked<() => Promise<Session | null>>(auth);

function SessionStatusProbe() {
  const { status, data } = useSession();
  return (
    <div>
      {status}: {data?.user.firstName ?? "none"}
    </div>
  );
}

describe("AuthSessionProvider", () => {
  test("primes useSession as authenticated so consumers skip the loading state", async () => {
    mockAuth.mockResolvedValue({
      user: {
        id: "11111111-1111-1111-1111-111111111111",
        firstName: "Session Primed",
        lastName: "User",
      },
      expires: "2099-01-01T00:00:00.000Z",
    });

    render(await AuthSessionProvider({ children: <SessionStatusProbe /> }));

    expect(
      screen.getByText("authenticated: Session Primed"),
    ).toBeInTheDocument();
  });

  test("primes useSession as unauthenticated (not loading) when there is no session", async () => {
    mockAuth.mockResolvedValue(null);

    render(await AuthSessionProvider({ children: <SessionStatusProbe /> }));

    expect(screen.getByText("unauthenticated: none")).toBeInTheDocument();
  });
});
