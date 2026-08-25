import { describe, expect, test, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import type { Session } from "@auth/core/types";

import { auth } from "@/auth";
import { generateMetadata, OrderDetailGate } from "./page";

vi.mock("@/auth", () => ({ auth: vi.fn() }));

vi.mock("./order-detail", () => ({
  OrderDetail: ({ orderNumber }: { orderNumber: string }) => (
    <div>Order detail stub for {orderNumber}</div>
  ),
}));

const mockAuth = vi.mocked<() => Promise<Session | null>>(auth);

function renderPage(orderNumber: string) {
  return OrderDetailGate({ params: Promise.resolve({ orderNumber }) });
}

describe("generateMetadata", () => {
  test("includes the order number in the title", async () => {
    const metadata = await generateMetadata({
      params: Promise.resolve({ orderNumber: "ORD-000001" }),
    });

    expect(metadata.title).toBe("Order ORD-000001");
  });
});

describe("OrderDetailGate", () => {
  test("renders the order detail when signed in", async () => {
    mockAuth.mockResolvedValue({
      user: {
        id: "11111111-1111-1111-1111-111111111111",
        firstName: "Session",
        lastName: "User",
        email: "session-user@test.invalid",
      },
      expires: "2099-01-01T00:00:00.000Z",
    });

    render(await renderPage("ORD-000001"));

    expect(
      screen.getByText("Order detail stub for ORD-000001"),
    ).toBeInTheDocument();
  });

  test("redirects to login, preserving the order number, when signed out", async () => {
    mockAuth.mockResolvedValue(null);

    await expect(renderPage("ORD-000001")).rejects.toMatchObject({
      digest:
        "NEXT_REDIRECT;replace;/login?next=/account/orders/ORD-000001;307;",
    });
  });
});
