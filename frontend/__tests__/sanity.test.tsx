import { expect, test } from "vitest";
import { render, screen } from "@testing-library/react";

function Greeting() {
  return <h1>Halcyon Records</h1>;
}

test("renders and queries the DOM", () => {
  render(<Greeting />);
  expect(
    screen.getByRole("heading", { name: "Halcyon Records" })
  ).toBeInTheDocument();
});