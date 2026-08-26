import { describe, expect, test, vi } from "vitest";

import { client } from "@/lib/api/client";
import { SITE_URL } from "@/lib/site-config";
import type { components } from "@/lib/api/schema";

import sitemap from "./sitemap";

vi.mock("@/lib/api/client", () => ({
  client: { GET: vi.fn() },
}));

vi.mock("next/server", () => ({
  connection: vi.fn(),
}));

type AlbumSummary = components["schemas"]["AlbumSummaryResponse"];
type ArtistListItem = components["schemas"]["ArtistListItemResponse"];
type GenreListItem = components["schemas"]["GenreListItemResponse"];
type DecadeListItem = components["schemas"]["DecadeListItemResponse"];

function buildAlbum(overrides: Partial<AlbumSummary> = {}): AlbumSummary {
  return {
    sqid: "sitemap-album-1",
    title: "Sitemap Album",
    titleSlug: "sitemap-album",
    imageUrl: null,
    releaseDate: "2024-03-01",
    priceInPence: 2400,
    originalPriceInPence: null,
    isNew: false,
    isOnSale: false,
    isStaffPick: false,
    unitsInStock: 10,
    isInStock: true,
    artists: [],
    genres: [],
    ...overrides,
  };
}

function buildArtist(overrides: Partial<ArtistListItem> = {}): ArtistListItem {
  return {
    sqid: "sitemap-artist-1",
    name: "Sitemap Artist",
    nameSlug: "sitemap-artist",
    albumCount: 1,
    ...overrides,
  };
}

function buildGenre(overrides: Partial<GenreListItem> = {}): GenreListItem {
  return {
    name: "Sitemap Genre",
    slug: "sitemap-genre",
    imageUrl: null,
    albumCount: 1,
    ...overrides,
  };
}

function buildDecade(overrides: Partial<DecadeListItem> = {}): DecadeListItem {
  return {
    slug: "sitemap-decade",
    label: "Sitemap Decade",
    startYear: 1990,
    endYear: 1999,
    imageUrl: null,
    albumCount: 1,
    ...overrides,
  };
}

interface CatalogOverrides {
  albumPages?: { items: AlbumSummary[]; totalPages: number }[];
  artists?: ArtistListItem[];
  genres?: GenreListItem[];
  decades?: DecadeListItem[];
}

function mockCatalog({
  albumPages = [{ items: [buildAlbum()], totalPages: 1 }],
  artists = [buildArtist()],
  genres = [buildGenre()],
  decades = [buildDecade()],
}: CatalogOverrides = {}) {
  vi.mocked(client.GET).mockImplementation((path, options) => {
    switch (path) {
      case "/api/albums": {
        const page = (options as { params?: { query?: { page?: number } } })
          ?.params?.query?.page as number;
        const { items, totalPages } = albumPages[page - 1];
        return Promise.resolve({
          data: {
            items,
            page,
            pageSize: 50,
            totalCount: items.length,
            totalPages,
          },
          error: undefined,
          response: new Response(),
        });
      }
      case "/api/artists":
        return Promise.resolve({
          data: artists,
          error: undefined,
          response: new Response(),
        });
      case "/api/genres":
        return Promise.resolve({
          data: genres,
          error: undefined,
          response: new Response(),
        });
      case "/api/decades":
        return Promise.resolve({
          data: decades,
          error: undefined,
          response: new Response(),
        });
      default:
        throw new Error(`Unexpected path in sitemap test: ${path}`);
    }
  }) as typeof client.GET;
}

describe("sitemap", () => {
  test("includes every static top-level route", async () => {
    mockCatalog();

    const urls = (await sitemap()).map((entry) => entry.url);

    expect(urls).toEqual(
      expect.arrayContaining([
        SITE_URL,
        `${SITE_URL}/shop`,
        `${SITE_URL}/artists`,
        `${SITE_URL}/genres`,
        `${SITE_URL}/decades`,
      ]),
    );
  });

  test("builds canonical album URLs and paginates through every page", async () => {
    mockCatalog({
      albumPages: [
        {
          items: [
            buildAlbum({ sqid: "page-one", titleSlug: "page-one-album" }),
          ],
          totalPages: 2,
        },
        {
          items: [
            buildAlbum({ sqid: "page-two", titleSlug: "page-two-album" }),
          ],
          totalPages: 2,
        },
      ],
    });

    const urls = (await sitemap()).map((entry) => entry.url);

    expect(client.GET).toHaveBeenCalledWith(
      "/api/albums",
      expect.objectContaining({ params: { query: { page: 1, pageSize: 50 } } }),
    );
    expect(client.GET).toHaveBeenCalledWith(
      "/api/albums",
      expect.objectContaining({ params: { query: { page: 2, pageSize: 50 } } }),
    );
    expect(urls).toEqual(
      expect.arrayContaining([
        `${SITE_URL}/albums/page-one/page-one-album`,
        `${SITE_URL}/albums/page-two/page-two-album`,
      ]),
    );
  });

  test("builds canonical artist, genre, and decade URLs", async () => {
    mockCatalog({
      artists: [
        buildArtist({ sqid: "sitemap-artist-1", nameSlug: "sitemap-artist" }),
      ],
      genres: [buildGenre({ slug: "sitemap-genre" })],
      decades: [buildDecade({ slug: "sitemap-decade" })],
    });

    const urls = (await sitemap()).map((entry) => entry.url);

    expect(urls).toEqual(
      expect.arrayContaining([
        `${SITE_URL}/artists/sitemap-artist-1/sitemap-artist`,
        `${SITE_URL}/genres/sitemap-genre`,
        `${SITE_URL}/decades/sitemap-decade`,
      ]),
    );
  });

  test("throws when an underlying fetch errors", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { title: "Server error" },
      response: new Response(),
    });

    await expect(sitemap()).rejects.toThrow();
  });
});
