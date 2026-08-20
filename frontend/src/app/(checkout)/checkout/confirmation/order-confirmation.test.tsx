import { beforeEach, describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { OrderConfirmation } from "./order-confirmation";

import type { components } from "@/lib/api/schema";

type OrderDetail = components["schemas"]["OrderDetailResponse"];

function fetchResponse(
  ok: boolean,
  body: unknown = null,
  status = ok ? 200 : 500,
): Response {
  return { ok, status, json: () => Promise.resolve(body) } as Response;
}

const order: OrderDetail = {
  orderNumber: "ORD-000001",
  placedAt: "2026-08-20T12:00:00Z",
  status: "Placed",
  contactFirstName: "Order",
  contactLastName: "Contact",
  contactEmail: "order-contact@test.invalid",
  totalInPence: 4000,
  items: [
    {
      albumSqid: "confirmation-album",
      title: "Confirmation Fixture Album",
      titleSlug: "confirmation-fixture-album",
      imageUrl: null,
      quantity: 2,
      priceAtPurchaseInPence: 2000,
    },
  ],
};

beforeEach(() => {
  vi.stubGlobal("fetch", vi.fn());
});

describe("OrderConfirmation", () => {
  test("fetches and renders the order once loaded", async () => {
    vi.mocked(fetch).mockResolvedValue(fetchResponse(true, order));

    render(<OrderConfirmation orderNumber="ORD-000001" />);

    expect(await screen.findByText("Order confirmed")).toBeInTheDocument();
    expect(screen.getByText(/ORD-000001/)).toBeInTheDocument();
    expect(screen.getByText("Confirmation Fixture Album")).toBeInTheDocument();
    expect(fetch).toHaveBeenCalledWith("/api/orders/ORD-000001");
  });

  test("shows a not-found message on a 404", async () => {
    vi.mocked(fetch).mockResolvedValue(fetchResponse(false, {}, 404));

    render(<OrderConfirmation orderNumber="ORD-000001" />);

    expect(
      await screen.findByText("We couldn't find that order."),
    ).toBeInTheDocument();
  });

  test("shows a generic error message on other failures", async () => {
    vi.mocked(fetch).mockResolvedValue(fetchResponse(false, {}, 500));

    render(<OrderConfirmation orderNumber="ORD-000001" />);

    expect(
      await screen.findByText(
        "Something went wrong loading your order. Please try again.",
      ),
    ).toBeInTheDocument();
  });
});
