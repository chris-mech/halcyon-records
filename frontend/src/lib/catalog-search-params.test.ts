import { describe, expect, test } from "vitest";

import {
  buildCatalogHref,
  parseCatalogFilters,
  type CatalogFilters,
} from "./catalog-search-params";

describe("parseCatalogFilters", () => {
  test("defaults an empty searchParams object", () => {
    expect(parseCatalogFilters({})).toEqual({ page: 1, sort: "NewestFirst" });
  });

  test.each([
    ["0", 1],
    ["-1", 1],
    ["abc", 1],
    ["", 1],
    ["3", 3],
  ])("normalises page=%s to %i", (raw, expected) => {
    expect(parseCatalogFilters({ page: raw }).page).toBe(expected);
  });

  test("falls back to page 1 on a repeated page key instead of NaN", () => {
    expect(parseCatalogFilters({ page: ["3", "5"] }).page).toBe(3);
  });

  test("falls back to NewestFirst for an unrecognised sort value", () => {
    expect(parseCatalogFilters({ sort: "Whatever" }).sort).toBe("NewestFirst");
  });

  test("accepts any of the six real sort values", () => {
    expect(parseCatalogFilters({ sort: "ArtistZA" }).sort).toBe("ArtistZA");
  });
});

describe("buildCatalogHref", () => {
  const base: CatalogFilters = { page: 1, sort: "NewestFirst" };

  test("returns the bare basePath when nothing is active", () => {
    expect(buildCatalogHref("/genres/jazz", base, {})).toBe("/genres/jazz");
  });

  test("resets page to 1 when changing sort from a later page", () => {
    const current = { ...base, page: 5 };
    expect(
      buildCatalogHref("/genres/jazz", current, { sort: "PriceAsc" }),
    ).toBe("/genres/jazz?sort=PriceAsc");
  });

  test("keeps an explicit page change instead of resetting it", () => {
    expect(buildCatalogHref("/genres/jazz", base, { page: 3 })).toBe(
      "/genres/jazz?page=3",
    );
  });

  test("omits sort from the URL when it's the default", () => {
    const current = { ...base, sort: "PriceAsc" as const };
    expect(
      buildCatalogHref("/genres/jazz", current, { sort: "NewestFirst" }),
    ).toBe("/genres/jazz");
  });

  test("respects a different basePath, e.g. a decade page", () => {
    expect(buildCatalogHref("/decades/1970s", base, { page: 2 })).toBe(
      "/decades/1970s?page=2",
    );
  });
});
