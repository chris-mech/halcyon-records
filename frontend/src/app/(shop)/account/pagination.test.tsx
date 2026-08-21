import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";

import { OrdersPagination } from "./pagination";

describe("OrdersPagination", () => {
  test("renders nothing when there is only one page", () => {
    const { container } = render(<OrdersPagination page={1} totalPages={1} />);
    expect(container).toBeEmptyDOMElement();
  });

  test("omits Prev on the first page and Next on the last page", () => {
    render(<OrdersPagination page={1} totalPages={3} />);
    expect(
      screen.queryByRole("link", { name: /prev/i }),
    ).not.toBeInTheDocument();
    expect(screen.getByRole("link", { name: /next/i })).toBeInTheDocument();
  });

  test("links each page number to /account?page=", () => {
    render(<OrdersPagination page={1} totalPages={3} />);
    expect(screen.getByRole("link", { name: "2" })).toHaveAttribute(
      "href",
      "/account?page=2",
    );
  });
});
