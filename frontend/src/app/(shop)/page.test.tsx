import { describe, expect, test, vi } from "vitest";
import { render, screen, within } from "@testing-library/react";

import { client } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

import { HomeContent } from "./page";

vi.mock("@/lib/api/client", () => ({
  client: { GET: vi.fn() },
}));

vi.mock("next/cache", () => ({
  cacheLife: vi.fn(),
}));

vi.mock("next/server", () => ({
  connection: vi.fn(),
}));

type CoverStory = components["schemas"]["CoverStoryResponse"];
type AlbumSummary = components["schemas"]["AlbumSummaryResponse"];

function buildCoverStory(overrides: Partial<CoverStory> = {}): CoverStory {
  return {
    sqid: "cover1",
    title: "Cover Story Album",
    titleSlug: "cover-story-album",
    description: "A pull-quote used to verify cover-story rendering.",
    imageUrl: null,
    releaseDate: "2001-02-01",
    priceInPence: 3200,
    originalPriceInPence: null,
    isNew: false,
    isOnSale: false,
    isStaffPick: true,
    unitsInStock: 10,
    isInStock: true,
    issueNumber: 14,
    artists: [
      {
        sqid: "art1",
        name: "Cover Story Artist",
        nameSlug: "cover-story-artist",
      },
    ],
    genres: [{ name: "Genre Match 1", slug: "genre-match-1" }],
    ...overrides,
  };
}

function buildAlbum(overrides: Partial<AlbumSummary> = {}): AlbumSummary {
  return {
    sqid: "abc123",
    title: "Base Album",
    titleSlug: "base-album",
    imageUrl: null,
    releaseDate: "2024-03-01",
    priceInPence: 2400,
    originalPriceInPence: null,
    isNew: false,
    isOnSale: false,
    isStaffPick: false,
    unitsInStock: 10,
    isInStock: true,
    artists: [{ sqid: "art1", name: "Base Artist", nameSlug: "base-artist" }],
    genres: [{ name: "Electronic", slug: "electronic" }],
    ...overrides,
  };
}

function mockHomepageFetch({
  coverStory = buildCoverStory(),
  newArrivals = [
    buildAlbum({ sqid: "new1", title: "New Arrival Album", isNew: true }),
  ],
  onSaleAlbums = [
    buildAlbum({ sqid: "sale1", title: "On Sale Album", isOnSale: true }),
  ],
  coverStoryError = false,
}: {
  coverStory?: CoverStory;
  newArrivals?: AlbumSummary[];
  onSaleAlbums?: AlbumSummary[];
  coverStoryError?: boolean;
} = {}) {
  vi.mocked(client.GET).mockImplementation(async (url, options) => {
    if (url === "/api/albums/cover-story") {
      return coverStoryError
        ? {
            data: undefined,
            error: { title: "Server Error", status: 500 },
            response: new Response(),
          }
        : { data: coverStory, error: undefined, response: new Response() };
    }

    const isNew = (options as { params?: { query?: { isNew?: boolean } } })
      ?.params?.query?.isNew;

    return {
      data: {
        items: isNew ? newArrivals : onSaleAlbums,
        page: 1,
        pageSize: 4,
        totalCount: isNew ? newArrivals.length : onSaleAlbums.length,
      },
      error: undefined,
      response: new Response(),
    };
  });
}

describe("HomePage", () => {
  test("renders the cover story", async () => {
    mockHomepageFetch();

    render(await HomeContent());

    expect(
      screen.getByRole("heading", { name: "Cover Story Album" }),
    ).toBeInTheDocument();
    expect(screen.getByText("Cover Story Artist")).toBeInTheDocument();
    expect(screen.getByText("£32.00")).toBeInTheDocument();
    expect(
      screen.getByText(/A pull-quote used to verify cover-story rendering/),
    ).toBeInTheDocument();
    expect(screen.getByText("No. 014")).toBeInTheDocument();
  });

  test("hides the cover-story image link from assistive tech, since the title link already carries it", async () => {
    mockHomepageFetch();

    const { container } = render(await HomeContent());

    const [imageLink] = container.querySelectorAll(
      'a[href="/albums/cover1/cover-story-album"]',
    );

    expect(imageLink).toHaveAttribute("aria-hidden", "true");
    expect(imageLink).toHaveAttribute("tabindex", "-1");
  });

  test("renders the new arrivals and on sale grids", async () => {
    mockHomepageFetch();

    render(await HomeContent());

    expect(
      screen.getByRole("heading", { name: "New arrivals" }),
    ).toBeInTheDocument();
    expect(screen.getByText("New Arrival Album")).toBeInTheDocument();

    expect(
      screen.getByRole("heading", { name: "On sale" }),
    ).toBeInTheDocument();
    expect(screen.getByText("On Sale Album")).toBeInTheDocument();
  });

  test("hides a grid section when its fetch returns no albums", async () => {
    mockHomepageFetch({ newArrivals: [] });

    render(await HomeContent());

    expect(
      screen.queryByRole("heading", { name: "New arrivals" }),
    ).not.toBeInTheDocument();
    expect(
      screen.getByRole("heading", { name: "On sale" }),
    ).toBeInTheDocument();
  });

  test("disables the cover story's Add to bag when it's out of stock", async () => {
    mockHomepageFetch({
      coverStory: buildCoverStory({ unitsInStock: 0, isInStock: false }),
    });

    render(await HomeContent());

    const coverStorySection = screen
      .getByRole("heading", { name: "Cover Story Album" })
      .closest("section")!;

    expect(
      within(coverStorySection).getByRole("button", { name: /add to bag/i }),
    ).toBeDisabled();
  });

  test("throws when the cover-story fetch fails", async () => {
    mockHomepageFetch({ coverStoryError: true });

    await expect(HomeContent()).rejects.toThrow("Failed to load the homepage.");
  });
});
