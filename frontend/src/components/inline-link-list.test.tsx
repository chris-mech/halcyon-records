import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";

import { InlineLinkList } from "./inline-link-list";

const items = [
  { id: "a1", href: "/artists/a1/artist-one", label: "Artist One" },
  { id: "a2", href: "/artists/a2/artist-two", label: "Artist Two" },
  { id: "a3", href: "/artists/a3/artist-three", label: "Artist Three" },
];

describe("InlineLinkList", () => {
  test("links every item to its own href", () => {
    render(<InlineLinkList items={items} />);

    expect(screen.getByRole("link", { name: "Artist One" })).toHaveAttribute(
      "href",
      "/artists/a1/artist-one",
    );
    expect(screen.getByRole("link", { name: "Artist Three" })).toHaveAttribute(
      "href",
      "/artists/a3/artist-three",
    );
  });

  test("separates items with a middot hidden from assistive technology", () => {
    const { container } = render(<InlineLinkList items={items} />);

    const separators = container.querySelectorAll('[aria-hidden="true"]');
    expect(separators).toHaveLength(2);
    separators.forEach((separator) => {
      expect(separator).toHaveTextContent("·");
    });
  });

  test("keeps a comma in the accessible text so names are not read as one run", () => {
    render(<InlineLinkList items={items} />);

    expect(screen.getAllByText(",")).toHaveLength(2);
  });

  test("renders no separator for a single item", () => {
    const { container } = render(<InlineLinkList items={items.slice(0, 1)} />);

    expect(container.querySelectorAll('[aria-hidden="true"]')).toHaveLength(0);
    expect(screen.queryByText(",")).not.toBeInTheDocument();
  });
});
