import { beforeEach, describe, expect, test, vi } from "vitest";

import { useCartStore } from "./cart-store";
import type { CartItem } from "./cart-store";
import { syncCart, syncCartOnLogout } from "./sync-cart";

function cartItem(overrides: Partial<CartItem> = {}): CartItem {
  return {
    albumSqid: "sync-cart-album",
    title: "Sync Cart Fixture Album",
    titleSlug: "sync-cart-fixture-album",
    imageUrl: null,
    priceInPence: 1500,
    originalPriceInPence: null,
    quantity: 1,
    unitsInStock: 5,
    isInStock: true,
    artists: [],
    ...overrides,
  };
}

function fetchResponse(ok: boolean, body: unknown = null): Response {
  return { ok, json: () => Promise.resolve(body) } as Response;
}

beforeEach(() => {
  useCartStore.setState({ items: [] });
  vi.restoreAllMocks();
  vi.spyOn(useCartStore.persist, "hasHydrated").mockReturnValue(true);
  vi.stubGlobal("fetch", vi.fn());
});

describe("syncCart", () => {
  test("pushes the local cart, then hydrates from the authoritative server cart", async () => {
    useCartStore.setState({ items: [cartItem({ quantity: 2 })] });
    const serverCart = [cartItem({ quantity: 5 })];
    vi.mocked(fetch)
      .mockResolvedValueOnce(fetchResponse(true))
      .mockResolvedValueOnce(fetchResponse(true, serverCart));

    expect(await syncCart()).toBe(true);

    expect(fetch).toHaveBeenNthCalledWith(1, "/api/cart/sync", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        items: [{ albumSqid: "sync-cart-album", quantity: 2 }],
      }),
    });
    expect(fetch).toHaveBeenNthCalledWith(2, "/api/cart");
    expect(useCartStore.getState().items).toEqual(serverCart);
  });

  test("skips the push and just hydrates when the local cart is empty", async () => {
    const serverCart = [cartItem({ quantity: 3 })];
    vi.mocked(fetch).mockResolvedValueOnce(fetchResponse(true, serverCart));

    expect(await syncCart()).toBe(true);

    expect(fetch).toHaveBeenCalledTimes(1);
    expect(fetch).toHaveBeenCalledWith("/api/cart");
    expect(useCartStore.getState().items).toEqual(serverCart);
  });

  test("leaves the local cart untouched when the push fails", async () => {
    const localItems = [cartItem({ quantity: 2 })];
    useCartStore.setState({ items: localItems });
    vi.mocked(fetch).mockResolvedValueOnce(fetchResponse(false));

    expect(await syncCart()).toBe(false);

    expect(fetch).toHaveBeenCalledTimes(1);
    expect(useCartStore.getState().items).toEqual(localItems);
  });

  test("leaves the local cart untouched when the get fails", async () => {
    const localItems = [cartItem({ quantity: 2 })];
    useCartStore.setState({ items: localItems });
    vi.mocked(fetch)
      .mockResolvedValueOnce(fetchResponse(true))
      .mockResolvedValueOnce(fetchResponse(false));

    expect(await syncCart()).toBe(false);

    expect(useCartStore.getState().items).toEqual(localItems);
  });

  test("waits for hydration to finish before syncing", async () => {
    vi.mocked(useCartStore.persist.hasHydrated).mockReturnValue(false);
    let finishHydration: (() => void) | undefined;
    vi.spyOn(useCartStore.persist, "onFinishHydration").mockImplementation(
      (callback) => {
        finishHydration = () => callback(useCartStore.getState());
        return () => {};
      },
    );
    vi.mocked(fetch).mockResolvedValue(fetchResponse(true, []));

    const syncPromise = syncCart();
    await Promise.resolve();
    expect(fetch).not.toHaveBeenCalled();

    finishHydration?.();
    await syncPromise;

    expect(fetch).toHaveBeenCalled();
  });
});

describe("syncCartOnLogout", () => {
  test("pushes the local cart to the server, then clears it locally", async () => {
    const localItems = [cartItem({ quantity: 2 })];
    useCartStore.setState({ items: localItems });
    vi.mocked(fetch).mockResolvedValueOnce(fetchResponse(true));

    await syncCartOnLogout();

    expect(fetch).toHaveBeenCalledWith("/api/cart/sync", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        items: [{ albumSqid: "sync-cart-album", quantity: 2 }],
      }),
    });
    expect(useCartStore.getState().items).toEqual([]);
  });

  test("clears the local cart even when the push fails", async () => {
    useCartStore.setState({ items: [cartItem({ quantity: 2 })] });
    vi.mocked(fetch).mockResolvedValueOnce(fetchResponse(false));

    await syncCartOnLogout();

    expect(useCartStore.getState().items).toEqual([]);
  });

  test("pushes an empty cart to the server too, so a just-emptied bag doesn't leave stale items behind", async () => {
    vi.mocked(fetch).mockResolvedValueOnce(fetchResponse(true));

    await syncCartOnLogout();

    expect(fetch).toHaveBeenCalledWith("/api/cart/sync", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ items: [] }),
    });
    expect(useCartStore.getState().items).toEqual([]);
  });
});
