import { describe, expect, test } from "vitest";
import { render, screen } from "@testing-library/react";

import { AccountShell } from "./account-shell";

describe("AccountShell", () => {
  test("renders the breadcrumb, heading, and tab content", () => {
    render(
      <AccountShell active="orders">
        <p>Tab content</p>
      </AccountShell>,
    );

    expect(screen.getByText("Home")).toBeInTheDocument();
    expect(screen.getByText("Account")).toBeInTheDocument();
    expect(screen.getByText("Your account")).toBeInTheDocument();
    expect(screen.getByText("Tab content")).toBeInTheDocument();
  });

  test("marks the active tab with aria-current", () => {
    render(
      <AccountShell active="details">
        <p>Tab content</p>
      </AccountShell>,
    );

    expect(
      screen.getByRole("link", { name: "Account details" }),
    ).toHaveAttribute("aria-current", "page");
    expect(
      screen.getByRole("link", { name: "Order history" }),
    ).not.toHaveAttribute("aria-current");
  });
});
