import { useEffect, useState } from "react";
import { create } from "zustand";
import {
  createJSONStorage,
  persist,
  type StateStorage,
} from "zustand/middleware";

import type { components } from "@/lib/api/schema";

type CartItem = components["schemas"]["CartItemResponse"];

interface CartState {
  items: CartItem[];
  addItem: (item: CartItem) => void;
  setItemQuantity: (albumSqid: string, quantity: number) => void;
  removeItem: (albumSqid: string) => void;
  setItems: (items: CartItem[]) => void;
}

const noopStorage: StateStorage = {
  getItem: () => null,
  setItem: () => {},
  removeItem: () => {},
};

const useCartStore = create<CartState>()(
  persist(
    (set) => ({
      items: [],

      addItem: (item) =>
        set((state) => {
          if (item.unitsInStock <= 0) {
            return state;
          }

          const existing = state.items.find(
            (existingItem) => existingItem.albumSqid === item.albumSqid,
          );

          if (!existing) {
            return {
              items: [
                ...state.items,
                {
                  ...item,
                  quantity: Math.min(item.quantity, item.unitsInStock),
                },
              ],
            };
          }

          return {
            items: state.items.map((existingItem) =>
              existingItem.albumSqid === item.albumSqid
                ? {
                    ...item,
                    quantity: Math.min(
                      existingItem.quantity + item.quantity,
                      item.unitsInStock,
                    ),
                  }
                : existingItem,
            ),
          };
        }),

      setItemQuantity: (albumSqid, quantity) =>
        set((state) => {
          if (quantity <= 0) {
            return {
              items: state.items.filter((item) => item.albumSqid !== albumSqid),
            };
          }

          return {
            items: state.items.map((item) =>
              item.albumSqid === albumSqid
                ? { ...item, quantity: Math.min(quantity, item.unitsInStock) }
                : item,
            ),
          };
        }),

      removeItem: (albumSqid) =>
        set((state) => ({
          items: state.items.filter((item) => item.albumSqid !== albumSqid),
        })),

      setItems: (items) => set({ items }),
    }),
    {
      name: "halcyon-cart",
      storage: createJSONStorage(() =>
        typeof window === "undefined" ? noopStorage : localStorage,
      ),
      partialize: (state) => ({ items: state.items }),
      skipHydration: true,
    },
  ),
);

function selectCartTotalQuantity(state: CartState): number {
  return state.items.reduce((total, item) => total + item.quantity, 0);
}

function useCartHydrated(): boolean {
  const [hydrated, setHydrated] = useState(false);

  useEffect(() => {
    const unsubHydrate = useCartStore.persist.onHydrate(() =>
      setHydrated(false),
    );
    const unsubFinishHydration = useCartStore.persist.onFinishHydration(() =>
      setHydrated(true),
    );

    useCartStore.persist.rehydrate();

    return () => {
      unsubHydrate();
      unsubFinishHydration();
    };
  }, []);

  return hydrated;
}

export { useCartStore, selectCartTotalQuantity, useCartHydrated };
export type { CartItem };
