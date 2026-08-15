import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";

import { EmptyState } from "./empty-state";

describe("EmptyState", () => {
  test("renders the icon, heading, and description", () => {
    render(
      <EmptyState
        icon={<span data-testid="icon" />}
        heading="Nothing here"
        description="A description of the empty state."
      />,
    );

    expect(screen.getByTestId("icon")).toBeInTheDocument();
    expect(
      screen.getByRole("heading", { name: "Nothing here" }),
    ).toBeInTheDocument();
    expect(
      screen.getByText("A description of the empty state."),
    ).toBeInTheDocument();
  });

  test("renders children when provided", () => {
    render(
      <EmptyState icon={<span />} heading="Nothing here" description="Copy.">
        <a href="/somewhere">A link</a>
      </EmptyState>,
    );

    expect(screen.getByRole("link", { name: "A link" })).toBeInTheDocument();
  });

  test("renders nothing extra when children are omitted", () => {
    const { container } = render(
      <EmptyState icon={<span />} heading="Nothing here" description="Copy." />,
    );

    expect(container.querySelectorAll("a")).toHaveLength(0);
  });
});
