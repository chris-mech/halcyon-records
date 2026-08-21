import { beforeEach, describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";

import { OrderHistory } from "./order-history";

import type { components } from "@/lib/api/schema";

type PagedOrders = components["schemas"]["PagedResultOfOrderSummaryResponse"];

function fetchResponse(
  ok: boolean,
  body: unknown = null,
  status = ok ? 200 : 500,
): Response {
  return { ok, status, json: () => Promise.resolve(body) } as Response;
}

const pagedOrders: PagedOrders = {
  items: [
    {
      orderNumber: "ORD-000001",
      placedAt: "2026-08-20T12:00:00Z",
      status: "Placed",
      totalInPence: 4000,
      items: [
        {
          albumSqid: "history-album",
          title: "Order History Fixture Album",
          titleSlug: "order-history-fixture-album",
          imageUrl: null,
        },
      ],
    },
  ],
  page: 1,
  pageSize: 10,
  totalCount: 1,
  totalPages: 1,
};

beforeEach(() => {
  vi.stubGlobal("fetch", vi.fn());
});

describe("OrderHistory", () => {
  test("fetches and renders orders once loaded", async () => {
    vi.mocked(fetch).mockResolvedValue(fetchResponse(true, pagedOrders));

    render(<OrderHistory page={1} />);

    expect(await screen.findByText("Order ORD-000001")).toBeInTheDocument();
    expect(fetch).toHaveBeenCalledWith("/api/orders?page=1&pageSize=10");
  });

  test("shows an empty state when there are no orders", async () => {
    vi.mocked(fetch).mockResolvedValue(
      fetchResponse(true, {
        items: [],
        page: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 0,
      }),
    );

    render(<OrderHistory page={1} />);

    expect(
      await screen.findByText("You haven't placed any orders yet"),
    ).toBeInTheDocument();
  });

  test("shows a generic error message on failure", async () => {
    vi.mocked(fetch).mockResolvedValue(fetchResponse(false, {}, 500));

    render(<OrderHistory page={1} />);

    expect(
      await screen.findByText(
        "Something went wrong loading your orders. Please try again.",
      ),
    ).toBeInTheDocument();
  });
});
