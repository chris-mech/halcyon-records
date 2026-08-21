import { beforeEach, describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { AccountDetails } from "./account-details";

import type { components } from "@/lib/api/schema";

type CurrentUser = components["schemas"]["CurrentUserResponse"];

function fetchResponse(
  ok: boolean,
  body: unknown = null,
  status = ok ? 200 : 500,
): Response {
  return { ok, status, json: () => Promise.resolve(body) } as Response;
}

const user: CurrentUser = {
  id: "11111111-1111-1111-1111-111111111111",
  email: "account-details-fixture@test.invalid",
  firstName: "Account",
  lastName: "Fixture",
  registeredAt: "2026-06-14T00:00:00Z",
};

beforeEach(() => {
  vi.stubGlobal("fetch", vi.fn());
});

describe("AccountDetails", () => {
  test("fetches and renders the current user's details", async () => {
    vi.mocked(fetch).mockResolvedValue(fetchResponse(true, user));

    render(<AccountDetails />);

    expect(await screen.findByText("Account Fixture")).toBeInTheDocument();
    expect(
      screen.getByText("account-details-fixture@test.invalid"),
    ).toBeInTheDocument();
    expect(screen.getByText("14 June 2026")).toBeInTheDocument();
    expect(fetch).toHaveBeenCalledWith("/api/auth/me");
  });

  test("shows a generic error message on a failed fetch", async () => {
    vi.mocked(fetch).mockResolvedValue(fetchResponse(false, {}, 500));

    render(<AccountDetails />);

    expect(
      await screen.findByText(
        "Something went wrong loading your account details. Please try again.",
      ),
    ).toBeInTheDocument();
  });
});
