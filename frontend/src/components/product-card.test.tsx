import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";

import { ProductCard } from "@/components/product-card";
import type { components } from "@/lib/api/schema";

type AlbumSummary = components["schemas"]["AlbumSummaryResponse"];

function buildAlbum(overrides: Partial<AlbumSummary> = {}): AlbumSummary {
  return {
    sqid: "abc123",
    title: "Base Album",
    titleSlug: "base-album",
    imageUrl: null,
    releaseDate: "2024-03-01",
    priceInPence: 2400,
    originalPriceInPence: null,
    isNew: false,
    isOnSale: false,
    isStaffPick: false,
    unitsInStock: 10,
    isInStock: true,
    artists: [{ sqid: "art1", name: "Base Artist", nameSlug: "base-artist" }],
    genres: [{ name: "Electronic", slug: "electronic" }],
    ...overrides,
  };
}

describe("ProductCard", () => {
  test("links each artist credit separately", () => {
    render(
      <ProductCard
        album={buildAlbum({
          artists: [
            { sqid: "a1", name: "Artist One", nameSlug: "artist-one" },
            { sqid: "a2", name: "Artist Two", nameSlug: "artist-two" },
          ],
        })}
      />,
    );

    expect(screen.getByRole("link", { name: "Artist One" })).toHaveAttribute(
      "href",
      "/artists/a1/artist-one",
    );
    expect(screen.getByRole("link", { name: "Artist Two" })).toHaveAttribute(
      "href",
      "/artists/a2/artist-two",
    );
  });

  test("shows the On Sale tag only when isOnSale is true", () => {
    const { rerender } = render(
      <ProductCard album={buildAlbum({ isOnSale: false })} />,
    );
    expect(screen.queryByText(/on sale/i)).not.toBeInTheDocument();

    rerender(<ProductCard album={buildAlbum({ isOnSale: true })} />);
    expect(screen.getByText(/on sale/i)).toBeInTheDocument();
  });

  test("renders a struck-through original price whenever one exists, independent of isOnSale", () => {
    render(
      <ProductCard
        album={buildAlbum({
          isOnSale: false,
          priceInPence: 2400,
          originalPriceInPence: 2800,
        })}
      />,
    );

    expect(screen.getByText("£28.00")).toBeInTheDocument();
    expect(screen.getByText("£24.00")).toBeInTheDocument();
    expect(screen.queryByText(/on sale/i)).not.toBeInTheDocument();
  });

  test("shows the genre link by default", () => {
    render(<ProductCard album={buildAlbum()} />);
    expect(screen.getByRole("link", { name: "Electronic" })).toHaveAttribute(
      "href",
      "/genres/electronic",
    );
  });

  test("hides the genre link when showGenre is false", () => {
    render(<ProductCard album={buildAlbum()} showGenre={false} />);
    expect(
      screen.queryByRole("link", { name: "Electronic" }),
    ).not.toBeInTheDocument();
  });

  test("links each genre separately when there is more than one", () => {
    render(
      <ProductCard
        album={buildAlbum({
          genres: [
            { name: "Electronic", slug: "electronic" },
            { name: "Pop", slug: "pop" },
          ],
        })}
      />,
    );

    expect(screen.getByRole("link", { name: "Electronic" })).toHaveAttribute(
      "href",
      "/genres/electronic",
    );
    expect(screen.getByRole("link", { name: "Pop" })).toHaveAttribute(
      "href",
      "/genres/pop",
    );
  });

  test("hides the release year by default", () => {
    render(<ProductCard album={buildAlbum({ releaseDate: "1974-06-01" })} />);
    expect(screen.queryByText("1974")).not.toBeInTheDocument();
  });

  test("shows the release year when showReleaseYear is true", () => {
    render(
      <ProductCard
        album={buildAlbum({ releaseDate: "1974-06-01" })}
        showReleaseYear
      />,
    );
    expect(screen.getByText("1974")).toBeInTheDocument();
  });

  test("renders a placeholder icon when there is no cover art", () => {
    render(<ProductCard album={buildAlbum({ imageUrl: null })} />);
    expect(screen.queryByRole("img")).not.toBeInTheDocument();
  });

  test("disables Add to cart when the album is out of stock", () => {
    render(<ProductCard album={buildAlbum({ isInStock: false })} />);
    expect(screen.getByRole("button", { name: /add to cart/i })).toBeDisabled();
  });

  test("keeps Add to cart enabled when in stock", () => {
    render(<ProductCard album={buildAlbum({ isInStock: true })} />);
    expect(screen.getByRole("button", { name: /add to cart/i })).toBeEnabled();
  });
});
