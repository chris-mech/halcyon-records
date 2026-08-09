import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";

import { FilterPills } from "./filter-pills";
import type { ShopFilters } from "./search-params";

const baseFilters: ShopFilters = {
  page: 1,
  isNew: false,
  isOnSale: false,
  isStaffPick: false,
  genres: [],
  sort: "NewestFirst",
};

describe("FilterPills", () => {
  test("marks All as current when no filter is active", () => {
    render(<FilterPills filters={baseFilters} />);
    expect(screen.getByRole("link", { name: "All" })).toHaveAttribute(
      "aria-current",
      "true",
    );
  });

  test("an active pill's own link turns it back off", () => {
    const filters = { ...baseFilters, isNew: true };
    render(<FilterPills filters={filters} />);
    const newIn = screen.getByRole("link", { name: "New in" });
    expect(newIn).toHaveAttribute("aria-current", "true");
    expect(newIn).toHaveAttribute("href", "/shop");
  });

  test("toggling one pill preserves the others already active", () => {
    const filters = { ...baseFilters, isStaffPick: true };
    render(<FilterPills filters={filters} />);
    expect(screen.getByRole("link", { name: "On sale" })).toHaveAttribute(
      "href",
      "/shop?isOnSale=true&isStaffPick=true",
    );
  });

  test("All resets every active filter at once", () => {
    const filters = { ...baseFilters, isNew: true, isOnSale: true };
    render(<FilterPills filters={filters} />);
    expect(screen.getByRole("link", { name: "All" })).toHaveAttribute(
      "href",
      "/shop",
    );
  });
});
