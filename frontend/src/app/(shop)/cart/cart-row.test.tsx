import { beforeEach, describe, expect, test } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";

import { useCartStore } from "@/lib/cart/cart-store";
import type { CartItem } from "@/lib/cart/cart-store";

import { CartRow } from "./cart-row";

function fixtureItem(overrides: Partial<CartItem> = {}): CartItem {
  return {
    albumSqid: "cart-row-album",
    title: "Cart Row Fixture Album",
    titleSlug: "cart-row-fixture-album",
    imageUrl: null,
    priceInPence: 1500,
    originalPriceInPence: null,
    quantity: 2,
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
});

describe("CartRow", () => {
  test("shows the line total for the given quantity", () => {
    render(<CartRow item={fixtureItem({ priceInPence: 1500, quantity: 2 })} />);

    expect(screen.getByText("£30.00")).toBeInTheDocument();
  });

  test("increasing quantity updates the store, capped at unitsInStock", () => {
    useCartStore.setState({
      items: [fixtureItem({ quantity: 5, unitsInStock: 5 })],
    });
    render(<CartRow item={fixtureItem({ quantity: 5, unitsInStock: 5 })} />);

    expect(
      screen.getByRole("button", { name: "Increase quantity" }),
    ).toBeDisabled();
  });

  test("clicking Remove removes the line from the store", () => {
    useCartStore.setState({
      items: [fixtureItem({ albumSqid: "cart-row-album" })],
    });
    render(<CartRow item={fixtureItem({ albumSqid: "cart-row-album" })} />);

    fireEvent.click(screen.getByRole("button", { name: "Remove" }));

    expect(useCartStore.getState().items).toEqual([]);
  });

  test("clicking Decrease reduces quantity by one", () => {
    useCartStore.setState({ items: [fixtureItem({ quantity: 2 })] });
    render(<CartRow item={fixtureItem({ quantity: 2 })} />);

    fireEvent.click(screen.getByRole("button", { name: "Decrease quantity" }));

    expect(useCartStore.getState().items[0].quantity).toBe(1);
  });
});
