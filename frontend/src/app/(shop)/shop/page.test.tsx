import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { client } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

import ShopPage from "./page";

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
  isInStock: true,
  artists: [{ sqid: "art1", name: "Loaded Artist", nameSlug: "loaded-artist" }],
  genres: [{ name: "Rock", slug: "rock" }],
};

function renderPage(searchParams: Record<string, string | string[]> = {}) {
  return ShopPage({
    params: Promise.resolve({}),
    searchParams: Promise.resolve(searchParams),
  });
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

    expect(screen.getByText("1 records")).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: "Loaded Album" }),
    ).toBeInTheDocument();
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

  test("throws when the API returns an error", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { title: "Server error" },
      response: new Response(),
    });

    await expect(renderPage()).rejects.toThrow("Failed to load albums.");
  });
});
