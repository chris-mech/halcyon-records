import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { client } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

import { GenresContent } from "./page";

vi.mock("@/lib/api/client", () => ({
  client: { GET: vi.fn() },
}));

vi.mock("next/cache", () => ({
  cacheLife: vi.fn(),
}));

vi.mock("next/server", () => ({
  connection: vi.fn(),
}));

type GenreListItem = components["schemas"]["GenreListItemResponse"];

function buildGenre(overrides: Partial<GenreListItem> = {}): GenreListItem {
  return {
    name: "Genre Match 1",
    slug: "genre-match-1",
    imageUrl: null,
    albumCount: 1,
    ...overrides,
  };
}

function mockGenresFetch(genres: GenreListItem[]) {
  vi.mocked(client.GET).mockResolvedValue({
    data: genres,
    error: undefined,
    response: new Response(),
  });
}

describe("GenresPage", () => {
  test("renders a tile linking to each genre", async () => {
    mockGenresFetch([
      buildGenre({ name: "Genre Match 1", slug: "genre-match-1" }),
      buildGenre({ name: "Genre Match 2", slug: "genre-match-2" }),
    ]);

    render(await GenresContent());

    expect(
      screen.getByRole("link", { name: /genre match 1/i }),
    ).toHaveAttribute("href", "/genres/genre-match-1");
    expect(
      screen.getByRole("link", { name: /genre match 2/i }),
    ).toHaveAttribute("href", "/genres/genre-match-2");
  });

  test("shows each tile's record count", async () => {
    mockGenresFetch([buildGenre({ albumCount: 7 })]);

    render(await GenresContent());

    expect(screen.getByText("7 records")).toBeInTheDocument();
  });

  test("throws when the genres fetch fails", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { title: "Server Error", status: 500 },
      response: new Response(),
    });

    await expect(GenresContent()).rejects.toThrow("Failed to load genres.");
  });
});
