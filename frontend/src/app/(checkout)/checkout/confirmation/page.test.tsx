import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { ConfirmationContent } from "./page";

vi.mock("./order-confirmation", () => ({
  OrderConfirmation: ({ orderNumber }: { orderNumber: string }) => (
    <div>Order confirmation stub for {orderNumber}</div>
  ),
}));

function renderPage(
  searchParams: Record<string, string | string[] | undefined>,
) {
  return ConfirmationContent({ searchParams: Promise.resolve(searchParams) });
}

describe("ConfirmationContent", () => {
  test("shows the confirmation step and passes the order number through", async () => {
    const { container } = render(await renderPage({ order: "ORD-000001" }));

    expect(
      screen.getByText("Order confirmation stub for ORD-000001"),
    ).toBeInTheDocument();
    expect(container.querySelector('[aria-current="step"]')).toHaveTextContent(
      "Confirmation",
    );
  });

  test("calls notFound when no order number is given", async () => {
    await expect(renderPage({})).rejects.toMatchObject({
      digest: "NEXT_HTTP_ERROR_FALLBACK;404",
    });
  });
});
