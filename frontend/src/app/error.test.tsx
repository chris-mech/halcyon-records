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
});
