import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";

import { CategoryTile } from "./category-tile";

describe("CategoryTile", () => {
  test("links to the given href", () => {
    render(
      <CategoryTile
        href="/genres/jazz"
        name="Jazz"
        imageUrl={null}
        albumCount={5}
      />,
    );
    expect(screen.getByRole("link", { name: /jazz/i })).toHaveAttribute(
      "href",
      "/genres/jazz",
    );
  });

  test("shows singular/plural record counts correctly", () => {
    const { rerender } = render(
      <CategoryTile
        href="/genres/jazz"
        name="Jazz"
        imageUrl={null}
        albumCount={1}
      />,
    );
    expect(screen.getByText("1 record")).toBeInTheDocument();

    rerender(
      <CategoryTile
        href="/genres/jazz"
        name="Jazz"
        imageUrl={null}
        albumCount={5}
      />,
    );
    expect(screen.getByText("5 records")).toBeInTheDocument();
  });

  test("renders a placeholder icon when there is no image", () => {
    render(
      <CategoryTile
        href="/genres/jazz"
        name="Jazz"
        imageUrl={null}
        albumCount={5}
      />,
    );
    expect(screen.queryByRole("img")).not.toBeInTheDocument();
  });
});
