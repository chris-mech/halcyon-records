import { beforeEach, describe, expect, test } from "vitest";

import { selectCartTotalQuantity, useCartStore } from "./cart-store";
import type { CartItem } from "./cart-store";

function cartItem(overrides: Partial<CartItem> = {}): CartItem {
  return {
    albumSqid: "cart-item-album",
    title: "Cart Fixture Album",
    titleSlug: "cart-fixture-album",
    imageUrl: null,
    priceInPence: 1500,
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
  localStorage.clear();
});

describe("useCartStore", () => {
  test("addItem inserts a new line", () => {
    useCartStore
      .getState()
      .addItem(cartItem({ albumSqid: "album-a", quantity: 2 }));

    expect(useCartStore.getState().items).toEqual([
      cartItem({ albumSqid: "album-a", quantity: 2 }),
    ]);
  });

  test("addItem sums quantity into an existing line, capped at unitsInStock", () => {
    useCartStore.setState({
      items: [cartItem({ albumSqid: "album-a", quantity: 3, unitsInStock: 5 })],
    });

    useCartStore
      .getState()
      .addItem(
        cartItem({ albumSqid: "album-a", quantity: 4, unitsInStock: 5 }),
      );

    expect(useCartStore.getState().items[0].quantity).toBe(5);
  });

  test("addItem is a no-op when the album is out of stock", () => {
    useCartStore
      .getState()
      .addItem(
        cartItem({ albumSqid: "album-a", quantity: 1, unitsInStock: 0 }),
      );

    expect(useCartStore.getState().items).toEqual([]);
  });

  test("setItemQuantity updates and caps at unitsInStock", () => {
    useCartStore.setState({
      items: [cartItem({ albumSqid: "album-a", quantity: 1, unitsInStock: 3 })],
    });

    useCartStore.getState().setItemQuantity("album-a", 10);

    expect(useCartStore.getState().items[0].quantity).toBe(3);
  });

  test("setItemQuantity removes the line when quantity drops to zero", () => {
    useCartStore.setState({ items: [cartItem({ albumSqid: "album-a" })] });

    useCartStore.getState().setItemQuantity("album-a", 0);

    expect(useCartStore.getState().items).toEqual([]);
  });

  test("removeItem removes the matching line only", () => {
    useCartStore.setState({
      items: [
        cartItem({ albumSqid: "album-a" }),
        cartItem({ albumSqid: "album-b" }),
      ],
    });

    useCartStore.getState().removeItem("album-a");

    expect(useCartStore.getState().items.map((item) => item.albumSqid)).toEqual(
      ["album-b"],
    );
  });

  test("setItems replaces the cart wholesale", () => {
    useCartStore.setState({ items: [cartItem({ albumSqid: "album-a" })] });

    useCartStore
      .getState()
      .setItems([cartItem({ albumSqid: "album-b", quantity: 2 })]);

    expect(useCartStore.getState().items).toEqual([
      cartItem({ albumSqid: "album-b", quantity: 2 }),
    ]);
  });

  test("selectCartTotalQuantity sums quantity across all lines", () => {
    useCartStore.setState({
      items: [
        cartItem({ albumSqid: "album-a", quantity: 2 }),
        cartItem({ albumSqid: "album-b", quantity: 3 }),
      ],
    });

    expect(selectCartTotalQuantity(useCartStore.getState())).toBe(5);
  });
});
