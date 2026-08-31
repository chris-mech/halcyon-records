import { describe, expect, test } from "vitest";
import { render } from "@testing-library/react";

import { SkeletonCardGrid, SkeletonLines } from "./skeleton-primitives";

describe("SkeletonCardGrid", () => {
  test("renders four placeholder pieces per card, for the default count", () => {
    const { container } = render(<SkeletonCardGrid />);

    expect(container.querySelectorAll('[data-slot="skeleton"]')).toHaveLength(
      8 * 4,
    );
  });

  test("renders the requested number of placeholder cards", () => {
    const { container } = render(<SkeletonCardGrid count={2} />);

    expect(container.querySelectorAll('[data-slot="skeleton"]')).toHaveLength(
      2 * 4,
    );
  });
});

describe("SkeletonLines", () => {
  test("renders the default number of lines", () => {
    const { container } = render(<SkeletonLines />);

    expect(container.querySelectorAll('[data-slot="skeleton"]')).toHaveLength(
      3,
    );
  });

  test("renders the requested number of lines", () => {
    const { container } = render(<SkeletonLines count={5} />);

    expect(container.querySelectorAll('[data-slot="skeleton"]')).toHaveLength(
      5,
    );
  });

  test("merges a custom className onto the container", () => {
    const { container } = render(<SkeletonLines className="mt-4" />);

    expect(container.firstElementChild).toHaveClass("mt-4");
  });
});
