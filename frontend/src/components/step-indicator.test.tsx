import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";

import { StepIndicator } from "./step-indicator";

describe("StepIndicator", () => {
  test("renders all four step labels", () => {
    render(<StepIndicator currentStep="checkout" />);
    expect(screen.getByText("Bag")).toBeInTheDocument();
    expect(screen.getByText("Log in")).toBeInTheDocument();
    expect(screen.getByText("Checkout")).toBeInTheDocument();
    expect(screen.getByText("Confirmation")).toBeInTheDocument();
  });

  test("marks the current step with aria-current=step", () => {
    render(<StepIndicator currentStep="checkout" />);
    expect(screen.getByText("Checkout")).toHaveAttribute(
      "aria-current",
      "step",
    );
    expect(screen.getByText("Bag")).not.toHaveAttribute("aria-current");
    expect(screen.getByText("Confirmation")).not.toHaveAttribute(
      "aria-current",
    );
  });

  test("shows a checkmark on completed steps only", () => {
    render(<StepIndicator currentStep="checkout" />);
    const hasCombinedText =
      (text: string) => (_: string, element: Element | null) =>
        element?.textContent === text;

    expect(screen.getByText(hasCombinedText("Bag ✓"))).toBeInTheDocument();
    expect(screen.getByText(hasCombinedText("Log in ✓"))).toBeInTheDocument();
    expect(
      screen.queryByText(hasCombinedText("Checkout ✓")),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText(hasCombinedText("Confirmation ✓")),
    ).not.toBeInTheDocument();
  });

  test("exposes the checkout progress nav landmark", () => {
    render(<StepIndicator currentStep="login" />);
    expect(
      screen.getByRole("navigation", { name: "Checkout progress" }),
    ).toBeInTheDocument();
  });
});
