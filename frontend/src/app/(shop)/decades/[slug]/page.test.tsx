import { describe, expect, test, vi } from "vitest";
import { render, screen, within } from "@testing-library/react";

import { client } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

import { DecadeLandingContent } from "./page";

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

type DecadeDetail = components["schemas"]["DecadeDetailResponse"];
type DecadeListItem = components["schemas"]["DecadeListItemResponse"];
type PagedAlbums = components["schemas"]["PagedResultOfAlbumSummaryResponse"];

const decade: DecadeDetail = {
  slug: "1970s",
  label: "1970s",
  startYear: 1970,
  endYear: 1979,
  description: "A description used to verify decade landing rendering.",
  albumCount: 1,
};

const siblingDecades: DecadeListItem[] = [
  {
    slug: "1970s",
    label: "1970s",
    startYear: 1970,
    endYear: 1979,
    imageUrl: null,
    albumCount: 1,
  },
  {
    slug: "1980s",
    label: "1980s",
    startYear: 1980,
    endYear: 1989,
    imageUrl: null,
    albumCount: 3,
  },
];

const albums: PagedAlbums = {
  items: [
    {
      sqid: "album1",
      title: "Decade Grid Album One",
      titleSlug: "decade-grid-album-one",
      imageUrl: null,
      releaseDate: "1974-06-01",
      priceInPence: 1999,
      originalPriceInPence: null,
      isNew: false,
      isOnSale: false,
      isStaffPick: false,
      isInStock: true,
      artists: [{ sqid: "art1", name: "Artist One", nameSlug: "artist-one" }],
      genres: [{ name: "Genre Match 1", slug: "genre-match-1" }],
    },
  ],
  page: 1,
  pageSize: 12,
  totalCount: 1,
  totalPages: 1,
};

function mockDecadeFetches({
  detailError = false,
  detailOverrides = {},
  decades: decadesOverride = siblingDecades,
  albumsOverride = albums,
}: {
  detailError?: boolean;
  detailOverrides?: Partial<DecadeDetail>;
  decades?: DecadeListItem[];
  albumsOverride?: PagedAlbums;
} = {}) {
  vi.mocked(client.GET).mockImplementation(((url: string) => {
    if (url === "/api/decades/{slug}") {
      return Promise.resolve(
        detailError
          ? {
              data: undefined,
              error: { title: "Not Found", status: 404 },
              response: new Response(),
            }
          : {
              data: { ...decade, ...detailOverrides },
              error: undefined,
              response: new Response(),
            },
      );
    }
    if (url === "/api/decades") {
      return Promise.resolve({
        data: decadesOverride,
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

function renderPage(
  slug = decade.slug,
  searchParams: Record<string, string> = {},
) {
  return DecadeLandingContent({
    params: Promise.resolve({ slug }),
    searchParams: Promise.resolve(searchParams),
  });
}

describe("DecadeLandingPage", () => {
  test("renders decade header, description, count, and the album grid", async () => {
    mockDecadeFetches();

    render(await renderPage());

    expect(screen.getByRole("heading", { name: "1970s" })).toBeInTheDocument();
    expect(
      screen.getByText(
        "A description used to verify decade landing rendering.",
      ),
    ).toBeInTheDocument();
    expect(screen.getByText("1 record")).toBeInTheDocument();
    expect(screen.getByText("Decade Grid Album One")).toBeInTheDocument();
  });

  test("shows each card's genre and release year, unlike the genre landing page", async () => {
    mockDecadeFetches();

    render(await renderPage());

    expect(
      screen.getByRole("link", { name: "Genre Match 1" }),
    ).toBeInTheDocument();
    expect(screen.getByText("1974")).toBeInTheDocument();
  });

  test("passes an open-ended year range through to the albums query for the earliest bucket", async () => {
    mockDecadeFetches({
      detailOverrides: {
        slug: "earlier",
        label: "1960s & earlier",
        startYear: null,
      },
    });

    render(await renderPage("earlier"));

    expect(
      screen.getByRole("heading", { name: "1960s & earlier" }),
    ).toBeInTheDocument();
  });

  test("highlights the current decade in the sibling jump-nav", async () => {
    mockDecadeFetches();

    render(await renderPage());

    const nav = screen.getByRole("navigation", {
      name: "Jump to another decade",
    });
    expect(within(nav).getByRole("link", { name: "1970s" })).toHaveAttribute(
      "aria-current",
      "page",
    );
    expect(
      within(nav).getByRole("link", { name: "1980s" }),
    ).not.toHaveAttribute("aria-current");
  });

  test("shows an empty state when the decade has no albums", async () => {
    mockDecadeFetches({
      albumsOverride: { ...albums, items: [], totalCount: 0, totalPages: 1 },
    });

    render(await renderPage());

    expect(
      screen.getByText("No records from this decade yet."),
    ).toBeInTheDocument();
  });

  test("calls notFound when the decade fetch errors", async () => {
    mockDecadeFetches({ detailError: true });

    await expect(renderPage()).rejects.toMatchObject({
      digest: "NEXT_HTTP_ERROR_FALLBACK;404",
    });
  });
});
