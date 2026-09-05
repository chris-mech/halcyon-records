import { beforeEach, describe, expect, test, vi } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { useSession } from "next-auth/react";

import { useCartStore } from "@/lib/cart/cart-store";
import type { CartItem } from "@/lib/cart/cart-store";
import { syncCart } from "@/lib/cart/sync-cart";

import { CheckoutForm } from "./checkout-form";

const push = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push }),
}));

vi.mock("next-auth/react", () => ({
  useSession: vi.fn(),
}));

vi.mock("@/lib/cart/sync-cart", () => ({
  syncCart: vi.fn(),
}));

function fixtureItem(overrides: Partial<CartItem> = {}): CartItem {
  return {
    albumSqid: "checkout-form-album",
    title: "Checkout Form Fixture Album",
    titleSlug: "checkout-form-fixture-album",
    imageUrl: null,
    priceInPence: 2000,
    originalPriceInPence: null,
    quantity: 1,
    unitsInStock: 5,
    isInStock: true,
    artists: [],
    ...overrides,
  };
}

function fetchResponse(
  ok: boolean,
  body: unknown = null,
  status = ok ? 200 : 400,
): Response {
  return { ok, status, json: () => Promise.resolve(body) } as Response;
}

function submit() {
  fireEvent.click(
    screen.getByRole("button", { name: /Place order \(demo, no charge\)/ }),
  );
}

beforeEach(() => {
  push.mockClear();
  vi.mocked(syncCart).mockResolvedValue(true);
  useCartStore.setState({
    items: [fixtureItem({ priceInPence: 2000, quantity: 1 })],
  });
  vi.mocked(useSession).mockReturnValue({
    status: "authenticated",
    data: {
      user: {
        id: "1",
        firstName: "Session First",
        lastName: "Session Last",
        email: "session-user@test.invalid",
      },
      expires: "2099-01-01T00:00:00.000Z",
    },
    update: vi.fn(),
  });
  vi.stubGlobal("fetch", vi.fn());
});

describe("CheckoutForm", () => {
  test("pre-fills the contact fields from the session", () => {
    render(<CheckoutForm />);

    expect(screen.getByLabelText("First name")).toHaveValue("Session First");
    expect(screen.getByLabelText("Last name")).toHaveValue("Session Last");
    expect(screen.getByLabelText("Email")).toHaveValue(
      "session-user@test.invalid",
    );
  });

  test("shows the cart subtotal on the submit button", () => {
    render(<CheckoutForm />);

    expect(
      screen.getByRole("button", {
        name: "Place order (demo, no charge): £20.00",
      }),
    ).toBeInTheDocument();
  });

  test("syncs the cart before submitting the order", async () => {
    vi.mocked(fetch).mockResolvedValue(
      fetchResponse(true, { orderNumber: "ORD-000001" }, 201),
    );
    render(<CheckoutForm />);

    submit();

    await waitFor(() => expect(fetch).toHaveBeenCalled());

    expect(syncCart).toHaveBeenCalled();
    expect(vi.mocked(syncCart).mock.invocationCallOrder[0]).toBeLessThan(
      vi.mocked(fetch).mock.invocationCallOrder[0],
    );
  });

  test("shows an error and does not submit the order when the cart sync fails", async () => {
    vi.mocked(syncCart).mockResolvedValueOnce(false);
    render(<CheckoutForm />);

    submit();

    expect(
      await screen.findByText("Couldn't refresh your cart. Please try again."),
    ).toBeInTheDocument();
    expect(fetch).not.toHaveBeenCalled();
    expect(push).not.toHaveBeenCalled();
  });

  test("submits the order, clears the cart, and redirects to the confirmation page", async () => {
    vi.mocked(fetch).mockResolvedValue(
      fetchResponse(true, { orderNumber: "ORD-000001" }, 201),
    );
    render(<CheckoutForm />);

    submit();

    await waitFor(() =>
      expect(push).toHaveBeenCalledWith(
        "/checkout/confirmation?order=ORD-000001",
      ),
    );
    expect(useCartStore.getState().items).toEqual([]);

    const [, options] = vi.mocked(fetch).mock.calls[0];
    const body = JSON.parse(options?.body as string);
    expect(body).toMatchObject({
      contactFirstName: "Session First",
      contactLastName: "Session Last",
      contactEmail: "session-user@test.invalid",
    });
    expect(typeof body.idempotencyKey).toBe("string");
    expect(body.idempotencyKey.length).toBeGreaterThan(0);
  });

  test("reuses the same idempotency key on a retried submit", async () => {
    vi.mocked(fetch).mockResolvedValue(fetchResponse(false, {}, 500));
    render(<CheckoutForm />);

    submit();
    await waitFor(() => expect(fetch).toHaveBeenCalledTimes(1));
    submit();
    await waitFor(() => expect(fetch).toHaveBeenCalledTimes(2));

    const firstBody = JSON.parse(
      vi.mocked(fetch).mock.calls[0][1]?.body as string,
    );
    const secondBody = JSON.parse(
      vi.mocked(fetch).mock.calls[1][1]?.body as string,
    );
    expect(secondBody.idempotencyKey).toBe(firstBody.idempotencyKey);
  });

  test("shows the sold-out message and refreshes the cart on a 409 conflict", async () => {
    vi.mocked(fetch).mockResolvedValue(
      fetchResponse(
        false,
        { detail: "Sorry, 'Sold Out Album' just sold out." },
        409,
      ),
    );
    render(<CheckoutForm />);

    submit();

    expect(
      await screen.findByText("Sorry, 'Sold Out Album' just sold out."),
    ).toBeInTheDocument();
    expect(syncCart).toHaveBeenCalled();
    expect(push).not.toHaveBeenCalled();
  });

  test("shows a generic error on other failures", async () => {
    vi.mocked(fetch).mockResolvedValue(fetchResponse(false, {}, 500));
    render(<CheckoutForm />);

    submit();

    expect(
      await screen.findByText(
        "Something went wrong placing your order. Please try again.",
      ),
    ).toBeInTheDocument();
    expect(push).not.toHaveBeenCalled();
  });
});
