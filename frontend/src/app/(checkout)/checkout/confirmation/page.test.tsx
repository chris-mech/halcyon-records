import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { ConfirmationContent, generateMetadata } from "./page";

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

describe("generateMetadata", () => {
  test("includes the order number in the title when present", async () => {
    const metadata = await generateMetadata({
      searchParams: Promise.resolve({ order: "ORD-000001" }),
    });

    expect(metadata.title).toBe("Order ORD-000001 Confirmed");
  });

  test("falls back to a generic title when no order number is given", async () => {
    const metadata = await generateMetadata({
      searchParams: Promise.resolve({}),
    });

    expect(metadata.title).toBe("Order Confirmed");
  });
});

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
