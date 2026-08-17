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

async function syncCart(): Promise<void> {
  await waitForCartHydration();

  const { items, setItems } = useCartStore.getState();

  if (items.length > 0) {
    const syncResponse = await fetch("/api/cart/sync", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        items: items.map((item) => ({
          albumSqid: item.albumSqid,
          quantity: item.quantity,
        })),
      }),
    });

    if (!syncResponse.ok) {
      return;
    }
  }

  const cartResponse = await fetch("/api/cart");

  if (!cartResponse.ok) {
    return;
  }

  const cartItems: CartItem[] = await cartResponse.json();
  setItems(cartItems);
}

export { syncCart };