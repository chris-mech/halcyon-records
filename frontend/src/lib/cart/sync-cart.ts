import { useCartStore } from "./cart-store";
import type { CartItem } from "./cart-store";

async function waitForCartHydration(): Promise<void> {
  if (useCartStore.persist.hasHydrated()) {
    return;
  }

  await new Promise<void>((resolve) => {
    const unsubscribe = useCartStore.persist.onFinishHydration(() => {
      unsubscribe();
      resolve();
    });
  });
}

async function pushLocalCart(
  items: CartItem[],
  { force = false }: { force?: boolean } = {},
): Promise<boolean> {
  if (items.length === 0 && !force) {
    return true;
  }

  const response = await fetch("/api/cart/sync", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      items: items.map((item) => ({
        albumSqid: item.albumSqid,
        quantity: item.quantity,
      })),
    }),
  });

  return response.ok;
}

async function syncCart(): Promise<void> {
  await waitForCartHydration();

  const { items, setItems } = useCartStore.getState();

  if (!(await pushLocalCart(items))) {
    return;
  }

  const cartResponse = await fetch("/api/cart");

  if (!cartResponse.ok) {
    return;
  }

  const cartItems: CartItem[] = await cartResponse.json();
  setItems(cartItems);
}

async function syncCartOnLogout(): Promise<void> {
  await waitForCartHydration();

  await pushLocalCart(useCartStore.getState().items, { force: true });
  useCartStore.getState().setItems([]);
}

export { syncCart, syncCartOnLogout };
