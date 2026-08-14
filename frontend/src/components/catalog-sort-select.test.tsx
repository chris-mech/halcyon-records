import { describe, expect, test, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";

import { CatalogSortSelect } from "./catalog-sort-select";
import type { CatalogFilters } from "@/lib/catalog-search-params";

const push = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push }),
}));

const baseFilters: CatalogFilters = { page: 1, sort: "NewestFirst" };

describe("CatalogSortSelect", () => {
  test("shows the current sort as the trigger's value", () => {
    render(<CatalogSortSelect basePath="/genres/jazz" filters={baseFilters} />);
    expect(screen.getByRole("combobox")).toHaveTextContent("Newest first");
  });

  test("lists all six sort options once opened", async () => {
    render(<CatalogSortSelect basePath="/genres/jazz" filters={baseFilters} />);
    fireEvent.click(screen.getByRole("combobox"));

    expect(
      await screen.findByRole("option", { name: "Artist Z–A" }),
    ).toBeInTheDocument();
    expect(screen.getAllByRole("option")).toHaveLength(6);
  });

  test("navigates to the new sort href for the given basePath", async () => {
    render(
      <CatalogSortSelect basePath="/decades/1970s" filters={baseFilters} />,
    );

    fireEvent.click(screen.getByRole("combobox"));
    const option = await screen.findByRole("option", {
      name: "Price: low to high",
    });
    fireEvent.pointerDown(option);
    fireEvent.click(option);

    expect(push).toHaveBeenCalledWith("/decades/1970s?sort=PriceAsc");
  });
});
