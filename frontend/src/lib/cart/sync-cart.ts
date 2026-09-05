import { toast } from "@/components/ui/toast";
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

async function pushLocalCart(items: CartItem[]): Promise<boolean> {
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

async function pullServerCart(): Promise<boolean> {
  const cartResponse = await fetch("/api/cart");

  if (!cartResponse.ok) {
    return false;
  }

  const cartItems: CartItem[] = await cartResponse.json();
  useCartStore.getState().setItems(cartItems);
  return true;
}

let inFlightSync: Promise<boolean> | null = null;

async function syncCart(): Promise<boolean> {
  if (inFlightSync) {
    return inFlightSync;
  }

  inFlightSync = performSync();
  try {
    return await inFlightSync;
  } finally {
    inFlightSync = null;
  }
}

async function performSync(): Promise<boolean> {
  await waitForCartHydration();

  const { items } = useCartStore.getState();

  if (!(await pushLocalCart(items))) {
    return false;
  }

  return pullServerCart();
}

async function mergeCartAtLogin(): Promise<boolean> {
  await waitForCartHydration();

  const { items } = useCartStore.getState();

  if (items.length === 0) {
    return pullServerCart();
  }

  if (!(await pushLocalCart(items))) {
    return false;
  }

  return pullServerCart();
}

async function syncCartOnLogout(): Promise<void> {
  await waitForCartHydration();

  await pushLocalCart(useCartStore.getState().items);
  useCartStore.getState().setItems([]);
}

function notifyCartSyncFailed(): void {
  toast.add({
    type: "error",
    title: "Cart didn't sync",
    description:
      "Your changes are saved locally, but couldn't reach the server. We'll try again shortly.",
  });
}

export { syncCart, mergeCartAtLogin, syncCartOnLogout, notifyCartSyncFailed };
