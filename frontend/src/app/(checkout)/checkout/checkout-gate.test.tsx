import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";

import { CheckoutGate } from "./checkout-gate";

describe("CheckoutGate", () => {
  test("links to login and register with a next=/checkout return path", () => {
    render(<CheckoutGate />);

    expect(screen.getByRole("link", { name: "Log in" })).toHaveAttribute(
      "href",
      "/login?next=/checkout",
    );
    expect(
      screen.getByRole("link", { name: "Create an account" }),
    ).toHaveAttribute("href", "/register?next=/checkout");
  });

  test("reassures the user their bag is saved", () => {
    render(<CheckoutGate />);

    expect(
      screen.getByText("Your bag is saved. Nothing will be lost."),
    ).toBeInTheDocument();
  });
});
