import { describe, expect, test, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";
import { signOut, useSession } from "next-auth/react";

import { Header } from "./header";

vi.mock("next-auth/react", () => ({
  useSession: vi.fn(),
  signOut: vi.fn(),
}));

describe("Header", () => {
  test("shows a Log in link when unauthenticated", () => {
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: "unauthenticated",
      update: vi.fn(),
    });

    render(<Header />);

    expect(screen.getByRole("link", { name: "Log in" })).toHaveAttribute(
      "href",
      "/login",
    );
  });

  test("shows the user's first name and a working Log out button when authenticated", () => {
    vi.mocked(useSession).mockReturnValue({
      data: {
        user: {
          id: "11111111-1111-1111-1111-111111111111",
          firstName: "Given Name Session",
          lastName: "Family Name Session",
        },
        expires: "2099-01-01T00:00:00.000Z",
      },
      status: "authenticated",
      update: vi.fn(),
    });

    render(<Header />);

    expect(screen.getByText("Given Name Session")).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Log out" }));
    expect(signOut).toHaveBeenCalled();
  });

  test("signs out automatically when the session carries a RefreshError", () => {
    vi.mocked(useSession).mockReturnValue({
      data: {
        user: {
          id: "11111111-1111-1111-1111-111111111111",
          firstName: "Given Name Session",
          lastName: "Family Name Session",
        },
        error: "RefreshError",
        expires: "2099-01-01T00:00:00.000Z",
      },
      status: "authenticated",
      update: vi.fn(),
    });

    render(<Header />);

    expect(signOut).toHaveBeenCalled();
  });
});
