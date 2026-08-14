import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";

import { CategoryPillNav } from "./category-pill-nav";

const items = [
  { key: "jazz", label: "Jazz", href: "/genres/jazz" },
  { key: "soul-funk", label: "Soul & Funk", href: "/genres/soul-funk" },
];

describe("CategoryPillNav", () => {
  test("renders a link for each item", () => {
    render(
      <CategoryPillNav
        ariaLabel="Jump to another genre"
        activeKey="jazz"
        items={items}
      />,
    );
    expect(screen.getByRole("link", { name: "Jazz" })).toHaveAttribute(
      "href",
      "/genres/jazz",
    );
    expect(screen.getByRole("link", { name: "Soul & Funk" })).toHaveAttribute(
      "href",
      "/genres/soul-funk",
    );
  });

  test("marks the active item with aria-current", () => {
    render(
      <CategoryPillNav
        ariaLabel="Jump to another genre"
        activeKey="jazz"
        items={items}
      />,
    );
    expect(screen.getByRole("link", { name: "Jazz" })).toHaveAttribute(
      "aria-current",
      "page",
    );
    expect(
      screen.getByRole("link", { name: "Soul & Funk" }),
    ).not.toHaveAttribute("aria-current");
  });

  test("uses the given aria-label on the nav landmark", () => {
    render(
      <CategoryPillNav
        ariaLabel="Jump to another decade"
        activeKey="jazz"
        items={items}
      />,
    );
    expect(
      screen.getByRole("navigation", { name: "Jump to another decade" }),
    ).toBeInTheDocument();
  });
});
