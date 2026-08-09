import { describe, expect, test } from "vitest";

import {
  buildShopHref,
  parseShopFilters,
  type ShopFilters,
} from "./search-params";

describe("parseShopFilters", () => {
  test("defaults an empty searchParams object", () => {
    expect(parseShopFilters({})).toEqual({
      page: 1,
      isNew: false,
      isOnSale: false,
      isStaffPick: false,
      genres: [],
      sort: "NewestFirst",
    });
  });

  test.each([
    ["0", 1],
    ["-1", 1],
    ["abc", 1],
    ["", 1],
    ["3", 3],
  ])("normalises page=%s to %i", (raw, expected) => {
    expect(parseShopFilters({ page: raw }).page).toBe(expected);
  });

  test("falls back to page 1 on a repeated page key instead of NaN", () => {
    expect(parseShopFilters({ page: ["3", "5"] }).page).toBe(3);
  });

  test.each([
    ["true", true],
    ["false", false],
    ["1", false],
    [undefined, false],
  ])(
    "only the literal string 'true' parses isNew=%s as %s",
    (raw, expected) => {
      expect(parseShopFilters({ isNew: raw }).isNew).toBe(expected);
    },
  );

  test("falls back to NewestFirst for an unrecognised sort value", () => {
    expect(parseShopFilters({ sort: "Whatever" }).sort).toBe("NewestFirst");
  });

  test("accepts any of the six real sort values", () => {
    expect(parseShopFilters({ sort: "ArtistZA" }).sort).toBe("ArtistZA");
  });

  test("normalises a single genre string to a one-item array", () => {
    expect(parseShopFilters({ genres: "rock" }).genres).toEqual(["rock"]);
  });

  test("keeps a genre array as-is, dropping empty entries", () => {
    expect(parseShopFilters({ genres: ["rock", "", "jazz"] }).genres).toEqual([
      "rock",
      "jazz",
    ]);
  });
});

describe("buildShopHref", () => {
  const base: ShopFilters = {
    page: 1,
    isNew: false,
    isOnSale: false,
    isStaffPick: false,
    genres: [],
    sort: "NewestFirst",
  };

  test("returns the bare path when nothing is active", () => {
    expect(buildShopHref(base, {})).toBe("/shop");
  });

  test("resets page to 1 when toggling a filter from a later page", () => {
    const current = { ...base, page: 5 };
    expect(buildShopHref(current, { isNew: true })).toBe("/shop?isNew=true");
  });

  test("keeps an explicit page change instead of resetting it", () => {
    expect(buildShopHref(base, { page: 3 })).toBe("/shop?page=3");
  });

  test("preserves other active filters when changing one", () => {
    const current = { ...base, isStaffPick: true };
    expect(buildShopHref(current, { isOnSale: true })).toBe(
      "/shop?isOnSale=true&isStaffPick=true",
    );
  });

  test("omits sort from the URL when it's the default", () => {
    const current = { ...base, sort: "PriceAsc" as const };
    expect(buildShopHref(current, { sort: "NewestFirst" })).toBe("/shop");
  });

  test("appends genres as repeated query keys", () => {
    expect(buildShopHref(base, { genres: ["rock", "jazz"] })).toBe(
      "/shop?genres=rock&genres=jazz",
    );
  });
});
