import { beforeEach, describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { useSession } from "next-auth/react";

import { useCartStore } from "@/lib/cart/cart-store";
import type { CartItem } from "@/lib/cart/cart-store";

import CartPage from "./page";

vi.mock("next-auth/react", () => ({
  useSession: vi.fn(),
}));

function fixtureItem(overrides: Partial<CartItem> = {}): CartItem {
  return {
    albumSqid: "cart-page-album",
    title: "Cart Page Fixture Album",
    titleSlug: "cart-page-fixture-album",
    imageUrl: null,
    priceInPence: 2000,
    originalPriceInPence: null,
    quantity: 1,
    unitsInStock: 5,
    isInStock: true,
    artists: [
      {
        sqid: "fixture-artist",
        name: "Fixture Artist",
        nameSlug: "fixture-artist",
      },
    ],
    ...overrides,
  };
}

beforeEach(() => {
  useCartStore.setState({ items: [] });
  vi.mocked(useSession).mockReturnValue({
    status: "unauthenticated",
    data: null,
    update: vi.fn(),
  });
});

describe("CartPage", () => {
  test("shows the empty state when the cart has no items", () => {
    render(<CartPage />);

    expect(screen.getByText("Your bag is empty")).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: "Start browsing" }),
    ).toHaveAttribute("href", "/shop");
  });

  test("shows bag rows and a subtotal when the cart has items", () => {
    useCartStore.setState({
      items: [
        fixtureItem({ albumSqid: "album-a", priceInPence: 2000, quantity: 1 }),
        fixtureItem({ albumSqid: "album-b", priceInPence: 1000, quantity: 2 }),
      ],
    });

    render(<CartPage />);

    expect(screen.getByText("3 items")).toBeInTheDocument();
    expect(screen.getAllByText("£40.00")).toHaveLength(2);
  });

  test("shows the login note for an anonymous user", () => {
    useCartStore.setState({ items: [fixtureItem()] });

    render(<CartPage />);

    expect(
      screen.getByText("You'll need to log in to complete checkout"),
    ).toBeInTheDocument();
  });

  test("hides the login note for an authenticated user", () => {
    useCartStore.setState({ items: [fixtureItem()] });
    vi.mocked(useSession).mockReturnValue({
      status: "authenticated",
      data: {
        user: { id: "1", firstName: "Fixture", lastName: "User" },
        expires: "2099-01-01T00:00:00.000Z",
      },
      update: vi.fn(),
    });

    render(<CartPage />);

    expect(
      screen.queryByText("You'll need to log in to complete checkout"),
    ).not.toBeInTheDocument();
  });
});
