import { beforeEach, describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { OrderDetail } from "./order-detail";

import type { components } from "@/lib/api/schema";

type Order = components["schemas"]["OrderDetailResponse"];

function fetchResponse(
  ok: boolean,
  body: unknown = null,
  status = ok ? 200 : 500,
): Response {
  return { ok, status, json: () => Promise.resolve(body) } as Response;
}

const order: Order = {
  orderNumber: "ORD-000001",
  placedAt: "2026-08-20T12:00:00Z",
  status: "Placed",
  contactFirstName: "Order",
  contactLastName: "Contact",
  contactEmail: "order-contact@test.invalid",
  totalInPence: 4000,
  items: [
    {
      albumSqid: "detail-album",
      title: "Order Detail Fixture Album",
      titleSlug: "order-detail-fixture-album",
      imageUrl: null,
      quantity: 2,
      priceAtPurchaseInPence: 2000,
    },
  ],
};

beforeEach(() => {
  vi.stubGlobal("fetch", vi.fn());
});

describe("OrderDetail", () => {
  test("fetches and renders the order once loaded, including contact details", async () => {
    vi.mocked(fetch).mockResolvedValue(fetchResponse(true, order));

    render(<OrderDetail orderNumber="ORD-000001" />);

    expect(await screen.findByText("Order ORD-000001")).toBeInTheDocument();
    expect(screen.getByText("Order Detail Fixture Album")).toBeInTheDocument();
    expect(screen.getByText("Order Contact")).toBeInTheDocument();
    expect(screen.getByText("order-contact@test.invalid")).toBeInTheDocument();
    expect(fetch).toHaveBeenCalledWith("/api/orders/ORD-000001");
  });

  test("shows a not-found message on a 404", async () => {
    vi.mocked(fetch).mockResolvedValue(fetchResponse(false, {}, 404));

    render(<OrderDetail orderNumber="ORD-000001" />);

    expect(
      await screen.findByText("We couldn't find that order."),
    ).toBeInTheDocument();
  });

  test("shows a generic error message on other failures", async () => {
    vi.mocked(fetch).mockResolvedValue(fetchResponse(false, {}, 500));

    render(<OrderDetail orderNumber="ORD-000001" />);

    expect(
      await screen.findByText(
        "Something went wrong loading your order. Please try again.",
      ),
    ).toBeInTheDocument();
  });
});
