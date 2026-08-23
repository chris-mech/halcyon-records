import { describe, expect, test, vi } from "vitest";
import { render, screen, within } from "@testing-library/react";

import { client } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

import { generateMetadata, GenreLandingContent } from "./page";

vi.mock("@/lib/api/client", () => ({
  client: { GET: vi.fn() },
}));

vi.mock("next/navigation", async (importOriginal) => {
  const actual = await importOriginal<typeof import("next/navigation")>();
  return {
    ...actual,
    useRouter: () => ({ push: vi.fn() }),
  };
});

vi.mock("next/cache", () => ({
  cacheLife: vi.fn(),
}));

type GenreDetail = components["schemas"]["GenreDetailResponse"];
type GenreListItem = components["schemas"]["GenreListItemResponse"];
type PagedAlbums = components["schemas"]["PagedResultOfAlbumSummaryResponse"];

const genre: GenreDetail = {
  name: "Full Detail Genre",
  slug: "full-detail-genre",
  description: "A description used to verify genre landing rendering.",
  imageUrl: null,
  albumCount: 1,
};

const siblingGenres: GenreListItem[] = [
  {
    name: "Full Detail Genre",
    slug: "full-detail-genre",
    description: null,
    imageUrl: null,
    albumCount: 1,
  },
  {
    name: "Genre Match 2",
    slug: "genre-match-2",
    description: null,
    imageUrl: null,
    albumCount: 3,
  },
];

const albums: PagedAlbums = {
  items: [
    {
      sqid: "album1",
      title: "Genre Grid Album One",
      titleSlug: "genre-grid-album-one",
      imageUrl: null,
      releaseDate: "2020-01-01",
      priceInPence: 1999,
      originalPriceInPence: null,
      isNew: false,
      isOnSale: false,
      isStaffPick: false,
      unitsInStock: 10,
      isInStock: true,
      artists: [{ sqid: "art1", name: "Artist One", nameSlug: "artist-one" }],
      genres: [{ name: "Full Detail Genre", slug: "full-detail-genre" }],
    },
  ],
  page: 1,
  pageSize: 12,
  totalCount: 1,
  totalPages: 1,
};

function mockGenreFetches({
  detailError = false,
  detailOverrides = {},
  genres: genresOverride = siblingGenres,
  albumsOverride = albums,
}: {
  detailError?: boolean;
  detailOverrides?: Partial<GenreDetail>;
  genres?: GenreListItem[];
  albumsOverride?: PagedAlbums;
} = {}) {
  vi.mocked(client.GET).mockImplementation(((url: string) => {
    if (url === "/api/genres/{slug}") {
      return Promise.resolve(
        detailError
          ? {
              data: undefined,
              error: { title: "Not Found", status: 404 },
              response: new Response(),
            }
          : {
              data: { ...genre, ...detailOverrides },
              error: undefined,
              response: new Response(),
            },
      );
    }
    if (url === "/api/genres") {
      return Promise.resolve({
        data: genresOverride,
        error: undefined,
        response: new Response(),
      });
    }
    if (url === "/api/albums") {
      return Promise.resolve({
        data: albumsOverride,
        error: undefined,
        response: new Response(),
      });
    }
    throw new Error(`Unexpected client.GET call: ${url}`);
  }) as typeof client.GET);
}

function renderMetadata(slug = genre.slug) {
  return generateMetadata({ params: Promise.resolve({ slug }) });
}

function renderPage(
  slug = genre.slug,
  searchParams: Record<string, string> = {},
) {
  return GenreLandingContent({
    params: Promise.resolve({ slug }),
    searchParams: Promise.resolve(searchParams),
  });
}

describe("generateMetadata", () => {
  test("uses the genre name and description", async () => {
    mockGenreFetches();

    const metadata = await renderMetadata();

    expect(metadata.title).toBe("Full Detail Genre");
    expect(metadata.description).toBe(
      "A description used to verify genre landing rendering.",
    );
  });

  test("sets the curated genre image as the Open Graph image", async () => {
    mockGenreFetches({
      detailOverrides: { imageUrl: "https://example.com/genre.jpg" },
    });

    const metadata = await renderMetadata();

    expect(metadata.openGraph?.images).toEqual([
      "https://example.com/genre.jpg",
    ]);
  });

  test("calls notFound when the genre fetch errors", async () => {
    mockGenreFetches({ detailError: true });

    await expect(renderMetadata()).rejects.toMatchObject({
      digest: "NEXT_HTTP_ERROR_FALLBACK;404",
    });
  });
});

describe("GenreLandingPage", () => {
  test("renders genre header, description, count, and the album grid", async () => {
    mockGenreFetches();

    render(await renderPage());

    expect(
      screen.getByRole("heading", { name: "Full Detail Genre" }),
    ).toBeInTheDocument();
    expect(
      screen.getByText("A description used to verify genre landing rendering."),
    ).toBeInTheDocument();
    expect(screen.getByText("1 record")).toBeInTheDocument();
    expect(screen.getByText("Genre Grid Album One")).toBeInTheDocument();
  });

  test("hides the redundant genre link on each card", async () => {
    mockGenreFetches();

    render(await renderPage());

    const anchors = screen
      .getAllByRole("link", { name: "Full Detail Genre" })
      .filter((el) => el.tagName === "A");
    expect(anchors).toHaveLength(1);
  });

  test("highlights the current genre in the sibling jump-nav", async () => {
    mockGenreFetches();

    render(await renderPage());

    const nav = screen.getByRole("navigation", {
      name: "Jump to another genre",
    });
    expect(
      within(nav).getByRole("link", { name: "Full Detail Genre" }),
    ).toHaveAttribute("aria-current", "page");
    expect(
      within(nav).getByRole("link", { name: "Genre Match 2" }),
    ).not.toHaveAttribute("aria-current");
  });

  test("shows an empty state when the genre has no albums", async () => {
    mockGenreFetches({
      albumsOverride: { ...albums, items: [], totalCount: 0, totalPages: 1 },
    });

    render(await renderPage());

    expect(
      screen.getByText("No records in this genre yet."),
    ).toBeInTheDocument();
  });

  test("calls notFound when the genre fetch errors", async () => {
    mockGenreFetches({ detailError: true });

    await expect(renderPage()).rejects.toMatchObject({
      digest: "NEXT_HTTP_ERROR_FALLBACK;404",
    });
  });
});
