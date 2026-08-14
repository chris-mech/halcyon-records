import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";

import { CatalogPagination } from "./catalog-pagination";
import type { CatalogFilters } from "@/lib/catalog-search-params";

const baseFilters: CatalogFilters = { page: 1, sort: "NewestFirst" };

describe("CatalogPagination", () => {
  test("renders nothing when there is only one page", () => {
    const { container } = render(
      <CatalogPagination
        basePath="/genres/jazz"
        filters={baseFilters}
        totalPages={1}
      />,
    );
    expect(container).toBeEmptyDOMElement();
  });

  test("omits Prev on the first page and Next on the last page", () => {
    render(
      <CatalogPagination
        basePath="/genres/jazz"
        filters={baseFilters}
        totalPages={3}
      />,
    );
    expect(
      screen.queryByRole("link", { name: /prev/i }),
    ).not.toBeInTheDocument();
    expect(screen.getByRole("link", { name: /next/i })).toBeInTheDocument();
  });

  test("builds page links against the given basePath, preserving sort", () => {
    const filters = { ...baseFilters, sort: "PriceAsc" as const };
    render(
      <CatalogPagination
        basePath="/decades/1970s"
        filters={filters}
        totalPages={3}
      />,
    );
    expect(screen.getByRole("link", { name: "2" })).toHaveAttribute(
      "href",
      "/decades/1970s?page=2&sort=PriceAsc",
    );
  });
});
