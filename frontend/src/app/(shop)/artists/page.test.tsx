import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { client } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

import { ArtistsContent } from "./page";

vi.mock("@/lib/api/client", () => ({
  client: { GET: vi.fn() },
}));

vi.mock("next/cache", () => ({
  cacheLife: vi.fn(),
}));

vi.mock("next/server", () => ({
  connection: vi.fn(),
}));

type ArtistListItem = components["schemas"]["ArtistListItemResponse"];

function buildArtist(overrides: Partial<ArtistListItem> = {}): ArtistListItem {
  return {
    sqid: "art1",
    name: "Artist One",
    nameSlug: "artist-one",
    albumCount: 1,
    ...overrides,
  };
}

function mockArtistsFetch(artists: ArtistListItem[]) {
  vi.mocked(client.GET).mockResolvedValue({
    data: artists,
    error: undefined,
    response: new Response(),
  });
}

describe("ArtistsPage", () => {
  test("groups artists under their first-letter heading", async () => {
    mockArtistsFetch([
      buildArtist({ sqid: "a1", name: "Artist One", nameSlug: "artist-one" }),
      buildArtist({ sqid: "b1", name: "Band Beta", nameSlug: "band-beta" }),
    ]);

    render(await ArtistsContent());

    expect(screen.getByRole("heading", { name: "A" })).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "B" })).toBeInTheDocument();
    expect(screen.getByText("Artist One")).toBeInTheDocument();
    expect(screen.getByText("Band Beta")).toBeInTheDocument();
  });

  test("buckets a non-letter name under the # heading", async () => {
    mockArtistsFetch([
      buildArtist({
        sqid: "n1",
        name: "3 O'Clock Static",
        nameSlug: "3-oclock-static",
      }),
    ]);

    render(await ArtistsContent());

    expect(screen.getByRole("heading", { name: "#" })).toBeInTheDocument();
    expect(screen.getByText("3 O'Clock Static")).toBeInTheDocument();
  });

  test("shows singular/plural album counts correctly", async () => {
    mockArtistsFetch([
      buildArtist({ sqid: "a1", albumCount: 1 }),
      buildArtist({
        sqid: "a2",
        name: "Artist Two",
        nameSlug: "artist-two",
        albumCount: 3,
      }),
    ]);

    render(await ArtistsContent());

    expect(screen.getByText("1 album")).toBeInTheDocument();
    expect(screen.getByText("3 albums")).toBeInTheDocument();
  });

  test("only renders a jump-nav link for letters that have artists", async () => {
    mockArtistsFetch([buildArtist({ name: "Artist One" })]);

    render(await ArtistsContent());

    expect(screen.getByRole("link", { name: "A" })).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "B" })).not.toBeInTheDocument();
    expect(screen.getByText("B")).toBeInTheDocument();
  });

  test("throws when the artists fetch fails", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { title: "Server Error", status: 500 },
      response: new Response(),
    });

    await expect(ArtistsContent()).rejects.toThrow("Failed to load artists.");
  });
});
