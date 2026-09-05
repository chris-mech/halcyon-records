import { beforeEach, describe, expect, test, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";

import { toast } from "@/components/ui/toast";
import { useCartStore } from "@/lib/cart/cart-store";

import { AddToCartButton } from "./add-to-cart-button";
import type { CartEligibleAlbum } from "./add-to-cart-button";

vi.mock("@/components/ui/toast", () => ({
  toast: { add: vi.fn() },
}));

function fixtureAlbum(
  overrides: Partial<CartEligibleAlbum> = {},
): CartEligibleAlbum {
  return {
    sqid: "add-to-cart-album",
    title: "Add To Cart Fixture Album",
    titleSlug: "add-to-cart-fixture-album",
    imageUrl: null,
    priceInPence: 1999,
    originalPriceInPence: null,
    unitsInStock: 3,
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
  vi.mocked(toast.add).mockClear();
});

describe("AddToCartButton", () => {
  test("adds the album to the cart and shows a confirmation toast", () => {
    render(<AddToCartButton album={fixtureAlbum()} />);

    fireEvent.click(screen.getByRole("button", { name: "Add to cart" }));

    expect(useCartStore.getState().items).toEqual([
      expect.objectContaining({ albumSqid: "add-to-cart-album", quantity: 1 }),
    ]);
    expect(toast.add).toHaveBeenCalledWith({
      title: "Added to cart",
      description: "Add To Cart Fixture Album",
    });
  });

  test("adds the given quantity instead of defaulting to one", () => {
    render(<AddToCartButton album={fixtureAlbum()} quantity={2} />);

    fireEvent.click(screen.getByRole("button", { name: "Add to cart" }));

    expect(useCartStore.getState().items[0].quantity).toBe(2);
  });

  test("is disabled when the album is out of stock", () => {
    render(<AddToCartButton album={fixtureAlbum({ isInStock: false })} />);

    expect(screen.getByRole("button", { name: "Add to cart" })).toBeDisabled();
  });

  test("is disabled once the cart already holds the full available stock", () => {
    useCartStore.setState({
      items: [
        {
          albumSqid: "add-to-cart-album",
          title: "Add To Cart Fixture Album",
          titleSlug: "add-to-cart-fixture-album",
          imageUrl: null,
          priceInPence: 1999,
          originalPriceInPence: null,
          quantity: 3,
          unitsInStock: 3,
          isInStock: true,
          artists: [],
        },
      ],
    });

    render(<AddToCartButton album={fixtureAlbum({ unitsInStock: 3 })} />);

    expect(screen.getByRole("button", { name: "Add to cart" })).toBeDisabled();
  });
});
