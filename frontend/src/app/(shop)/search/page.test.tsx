import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { client } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

import { SearchContent } from "./page";

vi.mock("@/lib/api/client", () => ({
  client: { GET: vi.fn() },
}));

type SearchAlbum = components["schemas"]["SearchAlbumResponse"];

const album: SearchAlbum = {
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

const suggestedAlbum: SearchAlbum = {
  ...album,
  sqid: "def456",
  title: "Suggested Album",
  titleSlug: "suggested-album",
};

function renderPage(searchParams: Record<string, string | string[]> = {}) {
  return SearchContent({ searchParams: Promise.resolve(searchParams) });
}

describe("SearchPage", () => {
  test("shows a search prompt with suggested terms when the query is blank", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: ["Suggested Term One", "Suggested Term Two"],
      error: undefined,
      response: new Response(),
    });

    render(await renderPage());

    expect(client.GET).toHaveBeenCalledWith("/api/search/suggestions");
    expect(
      screen.getByRole("heading", { name: "Search the catalogue" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: "Suggested Term One" }),
    ).toBeInTheDocument();
  });

  test("shows the search prompt with no suggestions when the suggestions call fails", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { title: "Server error" },
      response: new Response(),
    });

    render(await renderPage());

    expect(
      screen.getByRole("heading", { name: "Search the catalogue" }),
    ).toBeInTheDocument();
    expect(screen.queryByText("Try searching for")).not.toBeInTheDocument();
  });

  test("renders best matches and suggestions for a matching query", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: {
        bestMatches: [album],
        suggestions: [suggestedAlbum],
        suggestedTerms: [],
        totalCount: 1,
      },
      error: undefined,
      response: new Response(),
    });

    render(await renderPage({ q: "test query" }));

    expect(screen.getByText(/Results for/)).toBeInTheDocument();
    expect(screen.getByText("1 record found")).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: "Loaded Album" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: "Suggested Album" }),
    ).toBeInTheDocument();
  });

  test("shows the zero-results empty state with clickable suggested terms", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: {
        bestMatches: [],
        suggestions: [],
        suggestedTerms: ["Suggested Term One", "Suggested Term Two"],
        totalCount: 0,
      },
      error: undefined,
      response: new Response(),
    });

    render(await renderPage({ q: "no match query" }));

    expect(
      screen.getByRole("heading", { name: "Nothing turned up" }),
    ).toBeInTheDocument();
    const suggestedLink = screen.getByRole("link", {
      name: "Suggested Term One",
    });
    expect(suggestedLink).toHaveAttribute(
      "href",
      "/search?q=Suggested%20Term%20One",
    );
  });

  test("throws when the API returns an error", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { title: "Server error" },
      response: new Response(),
    });

    await expect(renderPage({ q: "test query" })).rejects.toThrow(
      "Search failed.",
    );
  });
});
