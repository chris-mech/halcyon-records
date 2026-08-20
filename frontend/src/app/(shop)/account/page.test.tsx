import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import type { Session } from "@auth/core/types";

import { auth } from "@/auth";
import { AccountContent } from "./page";

vi.mock("@/auth", () => ({ auth: vi.fn() }));

vi.mock("./order-history", () => ({
  OrderHistory: ({ page }: { page: number }) => (
    <div>Order history stub for page {page}</div>
  ),
}));

const mockAuth = vi.mocked<() => Promise<Session | null>>(auth);

function renderPage(
  searchParams: Record<string, string | string[] | undefined>,
) {
  return AccountContent({ searchParams: Promise.resolve(searchParams) });
}

describe("AccountContent", () => {
  test("renders order history for the requested page when signed in", async () => {
    mockAuth.mockResolvedValue({
      user: {
        id: "11111111-1111-1111-1111-111111111111",
        firstName: "Session",
        lastName: "User",
        email: "session-user@test.invalid",
      },
      expires: "2099-01-01T00:00:00.000Z",
    });

    render(await renderPage({ page: "3" }));

    expect(
      screen.getByText("Order history stub for page 3"),
    ).toBeInTheDocument();
  });

  test("defaults to page 1 when no page param is given", async () => {
    mockAuth.mockResolvedValue({
      user: {
        id: "11111111-1111-1111-1111-111111111111",
        firstName: "Session",
        lastName: "User",
        email: "session-user@test.invalid",
      },
      expires: "2099-01-01T00:00:00.000Z",
    });

    render(await renderPage({}));

    expect(
      screen.getByText("Order history stub for page 1"),
    ).toBeInTheDocument();
  });

  test("redirects to login, preserving the page param, when signed out", async () => {
    mockAuth.mockResolvedValue(null);

    await expect(renderPage({ page: "3" })).rejects.toMatchObject({
      digest: "NEXT_REDIRECT;replace;/login?next=/account?page=3;307;",
    });
  });

  test("redirects to plain /login?next=/account when signed out with no page param", async () => {
    mockAuth.mockResolvedValue(null);

    await expect(renderPage({})).rejects.toMatchObject({
      digest: "NEXT_REDIRECT;replace;/login?next=/account;307;",
    });
  });
});
