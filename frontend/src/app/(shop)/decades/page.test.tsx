import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { client } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

import { DecadesContent } from "./page";

vi.mock("@/lib/api/client", () => ({
  client: { GET: vi.fn() },
}));

vi.mock("next/cache", () => ({
  cacheLife: vi.fn(),
}));

vi.mock("next/server", () => ({
  connection: vi.fn(),
}));

type DecadeListItem = components["schemas"]["DecadeListItemResponse"];

function buildDecade(overrides: Partial<DecadeListItem> = {}): DecadeListItem {
  return {
    slug: "1970s",
    label: "1970s",
    startYear: 1970,
    endYear: 1979,
    imageUrl: null,
    albumCount: 1,
    ...overrides,
  };
}

function mockDecadesFetch(decades: DecadeListItem[]) {
  vi.mocked(client.GET).mockResolvedValue({
    data: decades,
    error: undefined,
    response: new Response(),
  });
}

describe("DecadesPage", () => {
  test("renders a tile linking to each decade", async () => {
    mockDecadesFetch([
      buildDecade({ slug: "1970s", label: "1970s" }),
      buildDecade({ slug: "1980s", label: "1980s" }),
    ]);

    render(await DecadesContent());

    expect(screen.getByRole("link", { name: /1970s/i })).toHaveAttribute(
      "href",
      "/decades/1970s",
    );
    expect(screen.getByRole("link", { name: /1980s/i })).toHaveAttribute(
      "href",
      "/decades/1980s",
    );
  });

  test("shows each tile's record count", async () => {
    mockDecadesFetch([buildDecade({ albumCount: 14 })]);

    render(await DecadesContent());

    expect(screen.getByText("14 records")).toBeInTheDocument();
  });

  test("renders an open-ended earliest bucket without a start year", async () => {
    mockDecadesFetch([
      buildDecade({
        slug: "earlier",
        label: "1960s & earlier",
        startYear: null,
        endYear: 1969,
      }),
    ]);

    render(await DecadesContent());

    expect(
      screen.getByRole("link", { name: /1960s & earlier/i }),
    ).toHaveAttribute("href", "/decades/earlier");
  });

  test("throws when the decades fetch fails", async () => {
    vi.mocked(client.GET).mockResolvedValue({
      data: undefined,
      error: { title: "Server Error", status: 500 },
      response: new Response(),
    });

    await expect(DecadesContent()).rejects.toThrow("Failed to load decades.");
  });
});
