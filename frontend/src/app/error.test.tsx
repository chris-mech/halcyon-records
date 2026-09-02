import { describe, expect, test, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";
import { useSession } from "next-auth/react";

import ErrorPage from "./error";

vi.mock("next-auth/react", () => ({
  useSession: vi.fn(),
  signOut: vi.fn(),
}));

describe("Error", () => {
  test("calls retry, not reset, when the try again button is clicked", () => {
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: "unauthenticated",
      update: vi.fn(),
    });

    const retry = vi.fn();

    render(<ErrorPage error={new Error("boom")} retry={retry} />);

    fireEvent.click(screen.getByRole("button", { name: "Try again" }));

    expect(retry).toHaveBeenCalledTimes(1);
  });

  test("shows the default message when the error has no backend-unavailable cause", () => {
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: "unauthenticated",
      update: vi.fn(),
    });

    render(<ErrorPage error={new Error("boom")} retry={vi.fn()} />);

    expect(
      screen.getByRole("heading", { name: "Something broke" }),
    ).toBeInTheDocument();
  });

  test("shows a backend-unavailable message when the error's cause carries a 503 status", () => {
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: "unauthenticated",
      update: vi.fn(),
    });

    const error = new Error("Failed to load the homepage.", {
      cause: { status: 503, error: { title: "Service Unavailable" } },
    });

    render(<ErrorPage error={error} retry={vi.fn()} />);

    expect(
      screen.getByRole("heading", { name: "The store is taking a moment" }),
    ).toBeInTheDocument();
  });
});
