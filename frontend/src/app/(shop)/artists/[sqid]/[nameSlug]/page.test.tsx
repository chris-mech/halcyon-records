import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { client } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

import { ArtistDetailContent, generateMetadata } from "./page";

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

type ArtistDetail = components["schemas"]["ArtistDetailResponse"];

const artist: ArtistDetail = {
  sqid: "detail1",
  name: "Full Detail Artist",
  nameSlug: "full-detail-artist",
  bio: "A bio used to verify artist detail rendering.",
  origin: "Test Origin",
  type: null,
  sinceYear: 2010,
  imageUrl: null,
  albumCount: 1,
  genres: [{ name: "Genre Match 1", slug: "genre-match-1" }],
  albums: [
    {
      sqid: "album1",
      title: "Discography Album One",
      titleSlug: "discography-album-one",
      imageUrl: null,
      releaseDate: "2010-05-01",
      priceInPence: 1999,
      originalPriceInPence: null,
      isNew: false,
      isOnSale: false,
      isStaffPick: false,
      unitsInStock: 10,
      isInStock: true,
      artists: [
        {
          sqid: "detail1",
          name: "Full Detail Artist",
          nameSlug: "full-detail-artist",
        },
      ],
      genres: [{ name: "Genre Match 1", slug: "genre-match-1" }],
    },
  ],
};

function mockArtistFetch(overrides: Partial<ArtistDetail> = {}) {
  vi.mocked(client.GET).mockResolvedValue({
    data: { ...artist, ...overrides },
    error: undefined,
    response: new Response(),
  });
}

function renderMetadata() {
  return generateMetadata({
    params: Promise.resolve({ sqid: artist.sqid, nameSlug: artist.nameSlug }),
    searchParams: Promise.resolve({}),
  });
}

function renderPage(nameSlug = artist.nameSlug, sort?: string) {
  return ArtistDetailContent({
    params: Promise.resolve({ sqid: artist.sqid, nameSlug }),
    searchParams: Promise.resolve(sort ? { sort } : {}),
  });
}

describe("generateMetadata", () => {
  test("uses the artist name as the title", async () => {
    mockArtistFetch();

    const metadata = await renderMetadata();

    expect(metadata.title).toBe("Full Detail Artist");
  });

  test("uses the bio as the description", async () => {
    mockArtistFetch();

    const metadata = await renderMetadata();

    expect(metadata.description).toBe(
      "A bio used to verify artist detail rendering.",
    );
  });

  test("sets the artist image as the Open Graph image", async () => {
    mockArtistFetch({ imageUrl: "https://example.com/artist.jpg" });

    const metadata = await renderMetadata();

    expect(metadata.openGraph?.images).toEqual([
      "https://example.com/artist.jpg",
    ]);
  });

  test("calls notFound when the artist fetch errors", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { title: "Not Found", status: 404 },
      response: new Response(),
    });

    await expect(renderMetadata()).rejects.toMatchObject({
      digest: "NEXT_HTTP_ERROR_FALLBACK;404",
    });
  });
});

describe("ArtistDetailPage", () => {
  test("renders artist detail on a successful load", async () => {
    mockArtistFetch();

    render(await renderPage());

    expect(
      screen.getByRole("heading", { name: "Full Detail Artist" }),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Test Origin · Active since 2010"),
    ).toBeInTheDocument();
    expect(
      screen.getByText("A bio used to verify artist detail rendering."),
    ).toBeInTheDocument();
    expect(screen.getByText("1 album in our catalogue")).toBeInTheDocument();
    expect(screen.getByText("Discography Album One")).toBeInTheDocument();
  });

  test("sets the artist's name as the image alt text", async () => {
    mockArtistFetch({ imageUrl: "https://example.com/artist.jpg" });

    render(await renderPage());

    expect(screen.getByAltText("Full Detail Artist")).toBeInTheDocument();
  });

  test("shows a Born label for a Person artist", async () => {
    mockArtistFetch({ type: "Person", sinceYear: 1985 });

    render(await renderPage());

    expect(screen.getByText(/Born 1985/)).toBeInTheDocument();
  });

  test("shows a Formed label for a Group artist", async () => {
    mockArtistFetch({ type: "Group", sinceYear: 2015 });

    render(await renderPage());

    expect(screen.getByText(/Formed 2015/)).toBeInTheDocument();
  });

  test("falls back to a generic Active since label when the type is unknown", async () => {
    mockArtistFetch({ type: null, sinceYear: 2015 });

    render(await renderPage());

    expect(screen.getByText(/Active since 2015/)).toBeInTheDocument();
  });

  test("shows every artist credit on a multi-artist album, and that album's own genre", async () => {
    mockArtistFetch({
      albums: [
        {
          ...artist.albums[0],
          artists: [
            {
              sqid: "detail1",
              name: "Full Detail Artist",
              nameSlug: "full-detail-artist",
            },
            {
              sqid: "collab1",
              name: "Collaborator Artist",
              nameSlug: "collaborator-artist",
            },
          ],
          genres: [{ name: "Genre Match 2", slug: "genre-match-2" }],
        },
      ],
    });

    render(await renderPage());

    const fullDetailArtistLinks = screen.getAllByRole("link", {
      name: "Full Detail Artist",
    });
    expect(
      fullDetailArtistLinks.some(
        (link) =>
          link.getAttribute("href") === "/artists/detail1/full-detail-artist",
      ),
    ).toBe(true);
    expect(
      screen.getByRole("link", { name: "Collaborator Artist" }),
    ).toHaveAttribute("href", "/artists/collab1/collaborator-artist");
    expect(screen.getByRole("link", { name: "Genre Match 2" })).toHaveAttribute(
      "href",
      "/genres/genre-match-2",
    );
  });

  test("shows only origin when since-year is missing", async () => {
    mockArtistFetch({ sinceYear: null });

    render(await renderPage());

    expect(screen.getByText("Test Origin")).toBeInTheDocument();
  });

  test("omits the origin note when both fields are missing", async () => {
    mockArtistFetch({ origin: null, sinceYear: null });

    render(await renderPage());

    expect(screen.queryByText(/Active since/)).not.toBeInTheDocument();
  });

  test("shows an empty-discography message when the artist has no albums", async () => {
    mockArtistFetch({ albums: [], albumCount: 0 });

    render(await renderPage());

    expect(
      screen.getByText("No albums in stock from this artist yet."),
    ).toBeInTheDocument();
    expect(screen.getByText("0 albums in our catalogue")).toBeInTheDocument();
  });

  test("defaults to NewestFirst when no sort param is present", async () => {
    mockArtistFetch();

    render(await renderPage());

    expect(client.GET).toHaveBeenCalledWith(
      "/api/artists/{sqid}",
      expect.objectContaining({
        params: expect.objectContaining({
          query: { sort: "NewestFirst" },
        }),
      }),
    );
  });

  test("falls back to NewestFirst on an invalid sort param", async () => {
    mockArtistFetch();

    render(await renderPage(artist.nameSlug, "NotARealSort"));

    expect(client.GET).toHaveBeenCalledWith(
      "/api/artists/{sqid}",
      expect.objectContaining({
        params: expect.objectContaining({
          query: { sort: "NewestFirst" },
        }),
      }),
    );
  });

  test("redirects permanently when the URL's name slug doesn't match", async () => {
    mockArtistFetch();

    await expect(renderPage("wrong-slug")).rejects.toMatchObject({
      digest: `NEXT_REDIRECT;replace;/artists/${artist.sqid}/${artist.nameSlug};308;`,
    });
  });

  test("calls notFound when the artist fetch errors", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { title: "Not Found", status: 404 },
      response: new Response(),
    });

    await expect(renderPage()).rejects.toMatchObject({
      digest: "NEXT_HTTP_ERROR_FALLBACK;404",
    });
  });
});
