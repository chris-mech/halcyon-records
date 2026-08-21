import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import type { Session } from "@auth/core/types";

import { auth } from "@/auth";
import { AccountDetailsContent } from "./page";

vi.mock("@/auth", () => ({ auth: vi.fn() }));

vi.mock("./account-details", () => ({
  AccountDetails: () => <div>Account details stub</div>,
}));

const mockAuth = vi.mocked<() => Promise<Session | null>>(auth);

describe("AccountDetailsContent", () => {
  test("renders account details when signed in", async () => {
    mockAuth.mockResolvedValue({
      user: {
        id: "11111111-1111-1111-1111-111111111111",
        firstName: "Session",
        lastName: "User",
        email: "session-user@test.invalid",
      },
      expires: "2099-01-01T00:00:00.000Z",
    });

    render(await AccountDetailsContent());

    expect(screen.getByText("Account details stub")).toBeInTheDocument();
  });

  test("redirects to login when signed out", async () => {
    mockAuth.mockResolvedValue(null);

    await expect(AccountDetailsContent()).rejects.toMatchObject({
      digest: "NEXT_REDIRECT;replace;/login?next=/account/details;307;",
    });
  });
});
