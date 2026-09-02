import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";

import { LoadingState } from "./loading-state";

describe("LoadingState", () => {
  test("renders children and an accessible loading status", () => {
    render(
      <LoadingState>
        <span data-testid="shape" />
      </LoadingState>,
    );

    expect(screen.getByRole("status")).toBeInTheDocument();
    expect(screen.getByText("Loading")).toBeInTheDocument();
    expect(screen.getByTestId("shape")).toBeInTheDocument();
  });

  test("hides children from the accessibility tree", () => {
    render(
      <LoadingState>
        <span data-testid="shape" />
      </LoadingState>,
    );

    expect(screen.getByTestId("shape").parentElement).toHaveAttribute(
      "aria-hidden",
      "true",
    );
  });

  test("marks itself for the cold-start overlay watcher", () => {
    render(
      <LoadingState>
        <span />
      </LoadingState>,
    );

    expect(screen.getByRole("status")).toHaveAttribute(
      "data-cold-start-skeleton",
      "true",
    );
  });
});
