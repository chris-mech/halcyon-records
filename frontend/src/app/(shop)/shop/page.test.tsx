import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { client } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

import { ShopContent } from "./page";

vi.mock("@/lib/api/client", () => ({
  client: { GET: vi.fn() },
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: vi.fn() }),
}));

type AlbumSummary = components["schemas"]["AlbumSummaryResponse"];

const album: AlbumSummary = {
  sqid: "abc123",
  title: "Loaded Album",
  titleSlug: "loaded-album",
  imageUrl: null,
  releaseDate: "2024-03-01",
  priceInPence: 1899,
  originalPriceInPence: null,
  isNew: false,
  isOnSale: false,
  isStaffPick: false,
  unitsInStock: 10,
  isInStock: true,
  artists: [{ sqid: "art1", name: "Loaded Artist", nameSlug: "loaded-artist" }],
  genres: [{ name: "Rock", slug: "rock" }],
};

const secondAlbum: AlbumSummary = {
  sqid: "def456",
  title: "Second Loaded Album",
  titleSlug: "second-loaded-album",
  imageUrl: null,
  releaseDate: "2024-03-01",
  priceInPence: 1899,
  originalPriceInPence: null,
  isNew: false,
  isOnSale: false,
  isStaffPick: false,
  unitsInStock: 10,
  isInStock: true,
  artists: [
    {
      sqid: "art2",
      name: "Second Loaded Artist",
      nameSlug: "second-loaded-artist",
    },
  ],
  genres: [{ name: "Rock", slug: "rock" }],
};

function renderPage(searchParams: Record<string, string | string[]> = {}) {
  return ShopContent({ searchParams: Promise.resolve(searchParams) });
}

describe("ShopPage", () => {
  test("renders the album grid and count on a successful load", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: {
        items: [album],
        page: 1,
        pageSize: 12,
        totalCount: 1,
        totalPages: 1,
      },
      error: undefined,
      response: new Response(),
    });

    render(await renderPage());

    expect(screen.getByText("1 record")).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: "Loaded Album" }),
    ).toBeInTheDocument();
  });

  test("pluralizes the record count for more than one album", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: {
        items: [album, secondAlbum],
        page: 1,
        pageSize: 12,
        totalCount: 2,
        totalPages: 1,
      },
      error: undefined,
      response: new Response(),
    });

    render(await renderPage());

    expect(screen.getByText("2 records")).toBeInTheDocument();
  });

  test("shows the empty-state message when no albums match", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: { items: [], page: 1, pageSize: 12, totalCount: 0, totalPages: 0 },
      error: undefined,
      response: new Response(),
    });

    render(await renderPage());

    expect(
      screen.getByText("No records match these filters."),
    ).toBeInTheDocument();
  });

  test("throws when the API returns an error, carrying status and error as the cause", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { title: "Server error" },
      response: new Response(null, { status: 503 }),
    });

    const error = (await renderPage().catch((e: unknown) => e)) as Error;

    expect(error).toBeInstanceOf(Error);
    expect(error.message).toBe("Failed to load albums.");
    expect(error.cause).toEqual({
      status: 503,
      error: { title: "Server error" },
    });
  });
});
