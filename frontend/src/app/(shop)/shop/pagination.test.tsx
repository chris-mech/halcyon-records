import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";

import { Pagination, getPageNumbers } from "./pagination";
import type { ShopFilters } from "./search-params";

const baseFilters: ShopFilters = {
  page: 1,
  isNew: false,
  isOnSale: false,
  isStaffPick: false,
  genres: [],
  sort: "NewestFirst",
};

describe("getPageNumbers", () => {
  test("shows every page when there are 7 or fewer", () => {
    expect(getPageNumbers(1, 5)).toEqual([1, 2, 3, 4, 5]);
  });

  test("collapses the middle into one ellipsis, matching the mockup's 1 2 3 … 16 shape", () => {
    expect(getPageNumbers(1, 16)).toEqual([1, 2, 3, "ellipsis", 16]);
  });

  test("keeps a window of two pages either side of the current page", () => {
    expect(getPageNumbers(8, 16)).toEqual([
      1,
      "ellipsis",
      6,
      7,
      8,
      9,
      10,
      "ellipsis",
      16,
    ]);
  });

  test("collapses only the leading gap when the current page is near the end", () => {
    expect(getPageNumbers(16, 16)).toEqual([1, "ellipsis", 14, 15, 16]);
  });
});

describe("Pagination", () => {
  test("renders nothing when there is only one page", () => {
    const { container } = render(
      <Pagination filters={baseFilters} totalPages={1} />,
    );
    expect(container).toBeEmptyDOMElement();
  });

  test("omits Prev on the first page and Next on the last page", () => {
    render(<Pagination filters={baseFilters} totalPages={3} />);
    expect(
      screen.queryByRole("link", { name: /prev/i }),
    ).not.toBeInTheDocument();
    expect(screen.getByRole("link", { name: /next/i })).toBeInTheDocument();
  });

  test("preserves active filters in every page link", () => {
    const filters = { ...baseFilters, isStaffPick: true };
    render(<Pagination filters={filters} totalPages={3} />);
    expect(screen.getByRole("link", { name: "2" })).toHaveAttribute(
      "href",
      "/shop?page=2&isStaffPick=true",
    );
  });
});
