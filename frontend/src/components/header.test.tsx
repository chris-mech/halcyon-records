import { describe, expect, test, vi, beforeEach } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { signOut, useSession } from "next-auth/react";

import { useCartStore } from "@/lib/cart/cart-store";

import { Header } from "./header";

vi.mock("next-auth/react", () => ({
  useSession: vi.fn(),
  signOut: vi.fn(),
}));

vi.mock("@/lib/cart/sync-cart", () => ({
  syncCartOnLogout: vi.fn().mockResolvedValue(undefined),
}));

beforeEach(() => {
  useCartStore.setState({ items: [] });
  localStorage.clear();
});

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

  test("shows the user's first name and a working Log out button when authenticated", async () => {
    vi.mocked(useSession).mockReturnValue({
      data: {
        user: {
          id: "11111111-1111-1111-1111-111111111111",
          firstName: "Given Name Session",
          lastName: "Family Name Session",
          email: "given-name-session@test.invalid",
        },
        expires: "2099-01-01T00:00:00.000Z",
      },
      status: "authenticated",
      update: vi.fn(),
    });

    render(<Header />);

    expect(screen.getByText("Given Name Session")).toBeInTheDocument();
    fireEvent.click(screen.getByRole("button", { name: "Log out" }));
    await waitFor(() => expect(signOut).toHaveBeenCalled());
  });

  test("signs out automatically when the session carries a RefreshError", async () => {
    vi.mocked(useSession).mockReturnValue({
      data: {
        user: {
          id: "11111111-1111-1111-1111-111111111111",
          firstName: "Given Name Session",
          lastName: "Family Name Session",
          email: "given-name-session@test.invalid",
        },
        error: "RefreshError",
        expires: "2099-01-01T00:00:00.000Z",
      },
      status: "authenticated",
      update: vi.fn(),
    });

    render(<Header />);

    await waitFor(() => expect(signOut).toHaveBeenCalled());
  });

  test("shows the live bag count from the cart store", () => {
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: "unauthenticated",
      update: vi.fn(),
    });
    useCartStore.setState({
      items: [
        {
          albumSqid: "header-count-album",
          title: "Header Count Fixture Album",
          titleSlug: "header-count-fixture-album",
          imageUrl: null,
          priceInPence: 1500,
          originalPriceInPence: null,
          quantity: 2,
          unitsInStock: 5,
          isInStock: true,
          artists: [],
        },
      ],
    });

    render(<Header />);

    expect(screen.getByRole("link", { name: "Bag (2)" })).toBeInTheDocument();
  });

  test("shows a Back to shop link in the stripped variant by default", () => {
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: "unauthenticated",
      update: vi.fn(),
    });

    render(<Header variant="stripped" />);

    expect(
      screen.getByRole("link", { name: "← Back to shop" }),
    ).toHaveAttribute("href", "/shop");
  });

  test("shows a custom back link when backHref and backLabel are given", () => {
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: "unauthenticated",
      update: vi.fn(),
    });

    render(
      <Header variant="stripped" backHref="/cart" backLabel="← Back to bag" />,
    );

    expect(screen.getByRole("link", { name: "← Back to bag" })).toHaveAttribute(
      "href",
      "/cart",
    );
  });
});
