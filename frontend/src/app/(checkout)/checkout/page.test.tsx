import { beforeEach, describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { useSession } from "next-auth/react";

import { useCartStore } from "@/lib/cart/cart-store";
import type { CartItem } from "@/lib/cart/cart-store";

import CheckoutPage from "./page";

vi.mock("next-auth/react", () => ({
  useSession: vi.fn(),
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: vi.fn() }),
}));

function fixtureItem(overrides: Partial<CartItem> = {}): CartItem {
  return {
    albumSqid: "checkout-page-album",
    title: "Checkout Page Fixture Album",
    titleSlug: "checkout-page-fixture-album",
    imageUrl: null,
    priceInPence: 2000,
    originalPriceInPence: null,
    quantity: 1,
    unitsInStock: 5,
    isInStock: true,
    artists: [],
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

describe("CheckoutPage", () => {
  test("shows the empty state when the cart has no items", () => {
    render(<CheckoutPage />);

    expect(screen.getByText("Your cart is empty")).toBeInTheDocument();
  });

  test("shows the login gate and step when unauthenticated", () => {
    useCartStore.setState({ items: [fixtureItem()] });

    const { container } = render(<CheckoutPage />);

    expect(screen.getByText("Log in to check out")).toBeInTheDocument();
    expect(container.querySelector('[aria-current="step"]')).toHaveTextContent(
      "Log in",
    );
  });

  test("shows the checkout form and step when authenticated", () => {
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

    const { container } = render(<CheckoutPage />);

    expect(screen.getByText("Order summary")).toBeInTheDocument();
    expect(container.querySelector('[aria-current="step"]')).toHaveTextContent(
      "Checkout",
    );
  });
});
