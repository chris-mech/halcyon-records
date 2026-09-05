import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { SITE_OPEN_GRAPH_DEFAULTS } from "@/lib/site-config";
import { client } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

import { AlbumDetailContent, generateMetadata } from "./page";

vi.mock("@/lib/api/client", () => ({
  client: { GET: vi.fn() },
}));

vi.mock("next/cache", () => ({
  cacheLife: vi.fn(),
}));

type AlbumDetail = components["schemas"]["AlbumDetailResponse"];
type RelatedAlbum = components["schemas"]["RelatedAlbumResponse"];

const album: AlbumDetail = {
  sqid: "detail1",
  title: "Full Detail Album",
  titleSlug: "full-detail-album",
  description: "A description used to verify detail rendering.",
  label: "Test Label",
  imageUrl: null,
  releaseDate: "1999-05-01",
  priceInPence: 1999,
  originalPriceInPence: null,
  isNew: false,
  isOnSale: false,
  isStaffPick: false,
  unitsInStock: 10,
  isInStock: true,
  artists: [{ sqid: "art1", name: "Artist One", nameSlug: "artist-one" }],
  genres: [{ name: "Genre Match 1", slug: "genre-match-1" }],
};

const relatedAlbum: RelatedAlbum = {
  sqid: "related1",
  title: "Related Album One",
  titleSlug: "related-album-one",
  imageUrl: null,
  releaseDate: "1999-05-01",
  priceInPence: 1599,
  originalPriceInPence: null,
  isNew: false,
  isOnSale: false,
  isStaffPick: false,
  unitsInStock: 10,
  isInStock: true,
  artists: [{ sqid: "art2", name: "Artist Two", nameSlug: "artist-two" }],
  genres: [{ name: "Genre Match 1", slug: "genre-match-1" }],
};

function mockAlbumFetch(
  overrides: Partial<AlbumDetail> = {},
  relatedAlbums: RelatedAlbum[] = [relatedAlbum],
) {
  vi.mocked(client.GET).mockImplementation(async (url) => {
    if (url === "/api/albums/{sqid}/related") {
      return {
        data: relatedAlbums,
        error: undefined,
        response: new Response(),
      };
    }
    return {
      data: { ...album, ...overrides },
      error: undefined,
      response: new Response(),
    };
  });
}

function renderMetadata() {
  return generateMetadata({
    params: Promise.resolve({ sqid: album.sqid, titleSlug: album.titleSlug }),
  });
}

function renderPage(titleSlug = album.titleSlug) {
  return AlbumDetailContent({
    params: Promise.resolve({ sqid: album.sqid, titleSlug }),
  });
}

describe("generateMetadata", () => {
  test("combines the album title and artist names", async () => {
    mockAlbumFetch();

    const metadata = await renderMetadata();

    expect(metadata.title).toBe("Full Detail Album by Artist One");
  });

  test("falls back to the bare title when there are no artists", async () => {
    mockAlbumFetch({ artists: [] });

    const metadata = await renderMetadata();

    expect(metadata.title).toBe("Full Detail Album");
  });

  test("sets the album cover as the Open Graph image", async () => {
    mockAlbumFetch({ imageUrl: "https://example.com/cover.jpg" });

    const metadata = await renderMetadata();

    expect(metadata.openGraph?.images).toEqual([
      "https://example.com/cover.jpg",
    ]);
  });

  test("keeps the site's Open Graph defaults alongside the cover image", async () => {
    mockAlbumFetch({ imageUrl: "https://example.com/cover.jpg" });

    const metadata = await renderMetadata();

    expect(metadata.openGraph).toMatchObject(SITE_OPEN_GRAPH_DEFAULTS);
  });

  test("leaves Open Graph unset when there is no cover image, so the site default applies", async () => {
    mockAlbumFetch({ imageUrl: null });

    const metadata = await renderMetadata();

    expect(metadata.openGraph).toBeUndefined();
  });

  test("calls notFound when the album fetch errors", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { title: "Not Found", status: 404 },
      response: new Response(null, { status: 404 }),
    });

    await expect(renderMetadata()).rejects.toMatchObject({
      digest: "NEXT_HTTP_ERROR_FALLBACK;404",
    });
  });
});

describe("AlbumDetailPage", () => {
  test("renders album detail on a successful load", async () => {
    mockAlbumFetch();

    render(await renderPage());

    expect(
      screen.getByRole("heading", { name: "Full Detail Album" }),
    ).toBeInTheDocument();
    expect(screen.getByText("Artist One")).toBeInTheDocument();
    expect(screen.getByText("£19.99")).toBeInTheDocument();
    expect(screen.getByText("Test Label")).toBeInTheDocument();
    expect(screen.getByText("1999")).toBeInTheDocument();
  });

  test("sets descriptive alt text on the album cover image", async () => {
    mockAlbumFetch({ imageUrl: "https://example.com/cover.jpg" });

    render(await renderPage());

    expect(
      screen.getByAltText("Full Detail Album by Artist One, album cover"),
    ).toBeInTheDocument();
  });

  test("renders a link for every genre on a multi-genre album", async () => {
    mockAlbumFetch(
      {
        genres: [
          { name: "Genre Match 1", slug: "genre-match-1" },
          { name: "Genre Match 2", slug: "genre-match-2" },
        ],
      },
      [{ ...relatedAlbum, genres: [] }],
    );

    render(await renderPage());

    expect(screen.getByRole("link", { name: "Genre Match 1" })).toHaveAttribute(
      "href",
      "/genres/genre-match-1",
    );
    expect(screen.getByRole("link", { name: "Genre Match 2" })).toHaveAttribute(
      "href",
      "/genres/genre-match-2",
    );
  });

  test("shows a low-stock note only below the threshold", async () => {
    mockAlbumFetch({ unitsInStock: 3 });

    render(await renderPage());

    expect(screen.getByText("Only 3 left in stock")).toBeInTheDocument();
  });

  test("shows no stock note when stock is healthy", async () => {
    mockAlbumFetch({ unitsInStock: 20 });

    render(await renderPage());

    expect(screen.queryByText(/left in stock/)).not.toBeInTheDocument();
  });

  test("shows out of stock and disables Add to cart at zero stock", async () => {
    mockAlbumFetch({ unitsInStock: 0, isInStock: false }, []);

    render(await renderPage());

    expect(screen.getByText("Out of stock")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Add to cart" })).toBeDisabled();
  });

  test("redirects permanently when the URL's title slug doesn't match", async () => {
    mockAlbumFetch();

    await expect(renderPage("wrong-slug")).rejects.toMatchObject({
      digest: `NEXT_REDIRECT;replace;/albums/${album.sqid}/${album.titleSlug};308;`,
    });
  });

  test("calls notFound when the album fetch errors", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { title: "Not Found", status: 404 },
      response: new Response(null, { status: 404 }),
    });

    await expect(renderPage()).rejects.toMatchObject({
      digest: "NEXT_HTTP_ERROR_FALLBACK;404",
    });
  });

  test("throws with cause when the album fetch fails with a non-404 error", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { title: "Server Error" },
      response: new Response(null, { status: 503 }),
    });

    const error = (await renderPage().catch((e: unknown) => e)) as Error;

    expect(error).toBeInstanceOf(Error);
    expect(error.message).toBe("Failed to load album.");
    expect(error.cause).toEqual({
      status: 503,
      error: { title: "Server Error" },
    });
  });

  test("degrades gracefully when the related-albums fetch fails", async () => {
    vi.mocked(client.GET).mockImplementation(async (url) => {
      if (url === "/api/albums/{sqid}/related") {
        return {
          data: undefined,
          error: { title: "Server Error", status: 500 },
          response: new Response(),
        };
      }
      return { data: album, error: undefined, response: new Response() };
    });

    render(await renderPage());

    expect(
      screen.getByRole("heading", { name: "Full Detail Album" }),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole("heading", { name: "More in this mood" }),
    ).not.toBeInTheDocument();
  });

  test("renders the related-albums grid on success", async () => {
    mockAlbumFetch();

    render(await renderPage());

    expect(
      screen.getByRole("heading", { name: "More in this mood" }),
    ).toBeInTheDocument();
    expect(screen.getByText("Related Album One")).toBeInTheDocument();
  });

  test("shows genre links on related-album cards", async () => {
    mockAlbumFetch();

    render(await renderPage());

    const genreLinks = screen.getAllByRole("link", { name: "Genre Match 1" });
    expect(genreLinks.length).toBeGreaterThanOrEqual(2);
    for (const link of genreLinks) {
      expect(link).toHaveAttribute("href", "/genres/genre-match-1");
    }
  });
});
