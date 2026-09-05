import { beforeEach, describe, expect, test, vi } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { useSession } from "next-auth/react";

import { useCartStore } from "@/lib/cart/cart-store";
import type { CartItem } from "@/lib/cart/cart-store";
import { notifyCartSyncFailed, syncCart } from "@/lib/cart/sync-cart";

import CartPage from "./page";

vi.mock("next-auth/react", () => ({
  useSession: vi.fn(),
}));

vi.mock("@/lib/cart/sync-cart", () => ({
  syncCart: vi.fn(),
  notifyCartSyncFailed: vi.fn(),
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
  vi.mocked(syncCart).mockClear().mockResolvedValue(true);
  vi.mocked(notifyCartSyncFailed).mockClear();
  vi.mocked(useSession).mockReturnValue({
    status: "unauthenticated",
    data: null,
    update: vi.fn(),
  });
});

describe("CartPage", () => {
  test("shows the empty state when the cart has no items", () => {
    render(<CartPage />);

    expect(screen.getByText("Your cart is empty")).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: "Start browsing" }),
    ).toHaveAttribute("href", "/");
  });

  test("shows cart rows and a subtotal when the cart has items", () => {
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
        user: {
          id: "1",
          firstName: "Fixture",
          lastName: "User",
          email: "fixture-user@test.invalid",
        },
        expires: "2099-01-01T00:00:00.000Z",
      },
      update: vi.fn(),
    });

    render(<CartPage />);

    expect(
      screen.queryByText("You'll need to log in to complete checkout"),
    ).not.toBeInTheDocument();
  });

  test("syncs the cart on mount when authenticated", () => {
    vi.mocked(useSession).mockReturnValue({
      status: "authenticated",
      data: {
        user: {
          id: "1",
          firstName: "Fixture",
          lastName: "User",
          email: "fixture-user@test.invalid",
        },
        expires: "2099-01-01T00:00:00.000Z",
      },
      update: vi.fn(),
    });

    render(<CartPage />);

    expect(syncCart).toHaveBeenCalledTimes(1);
  });

  test("does not sync when signed out", () => {
    render(<CartPage />);

    expect(syncCart).not.toHaveBeenCalled();
  });

  test("re-syncs when the window regains focus", () => {
    vi.mocked(useSession).mockReturnValue({
      status: "authenticated",
      data: {
        user: {
          id: "1",
          firstName: "Fixture",
          lastName: "User",
          email: "fixture-user@test.invalid",
        },
        expires: "2099-01-01T00:00:00.000Z",
      },
      update: vi.fn(),
    });

    render(<CartPage />);
    expect(syncCart).toHaveBeenCalledTimes(1);

    fireEvent(window, new Event("focus"));

    expect(syncCart).toHaveBeenCalledTimes(2);
  });

  test("shows a toast when the sync fails", async () => {
    vi.mocked(syncCart).mockResolvedValue(false);
    vi.mocked(useSession).mockReturnValue({
      status: "authenticated",
      data: {
        user: {
          id: "1",
          firstName: "Fixture",
          lastName: "User",
          email: "fixture-user@test.invalid",
        },
        expires: "2099-01-01T00:00:00.000Z",
      },
      update: vi.fn(),
    });

    render(<CartPage />);

    await waitFor(() => expect(notifyCartSyncFailed).toHaveBeenCalled());
  });
});
