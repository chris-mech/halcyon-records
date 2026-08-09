import { describe, expect, test, vi } from "vitest";
import { fireEvent, render, screen } from "@testing-library/react";

import { SortSelect } from "./sort-select";
import type { ShopFilters } from "./search-params";

const push = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push }),
}));

const baseFilters: ShopFilters = {
  page: 1,
  isNew: false,
  isOnSale: false,
  isStaffPick: false,
  genres: [],
  sort: "NewestFirst",
};

describe("SortSelect", () => {
  test("shows the current sort as the trigger's value", () => {
    render(<SortSelect filters={baseFilters} />);
    expect(screen.getByRole("combobox")).toHaveTextContent("Newest first");
  });

  test("lists all six sort options once opened", async () => {
    render(<SortSelect filters={baseFilters} />);
    fireEvent.click(screen.getByRole("combobox"));

    expect(
      await screen.findByRole("option", { name: "Artist Z–A" }),
    ).toBeInTheDocument();
    expect(screen.getAllByRole("option")).toHaveLength(6);
  });

  test("navigates to the new sort href when an option is chosen, preserving active filters", async () => {
    const filters = { ...baseFilters, isStaffPick: true };
    render(<SortSelect filters={filters} />);

    fireEvent.click(screen.getByRole("combobox"));
    const option = await screen.findByRole("option", {
      name: "Price: low to high",
    });
    fireEvent.pointerDown(option);
    fireEvent.click(option);

    expect(push).toHaveBeenCalledWith("/shop?isStaffPick=true&sort=PriceAsc");
  });
});
