"use client";

import Link from "next/link";
import { useSession } from "next-auth/react";
import { useEffect } from "react";
import { ShoppingBag } from "lucide-react";

import { buttonVariants } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { EmptyState } from "@/components/empty-state";
import { syncCart, notifyCartSyncFailed } from "@/lib/cart/sync-cart";
import {
  selectCartTotalQuantity,
  useCartHydrated,
  useCartStore,
} from "@/lib/cart/cart-store";
import { formatPrice } from "@/lib/format";
import { cn } from "@/lib/utils";

import { CartRow } from "./cart-row";

export default function CartPage() {
  const hydrated = useCartHydrated();
  const items = useCartStore((state) => state.items);
  const totalQuantity = useCartStore(selectCartTotalQuantity);
  const { status } = useSession();

  useEffect(() => {
    if (status !== "authenticated") {
      return;
    }

    async function runSync() {
      if (!(await syncCart())) {
        notifyCartSyncFailed();
      }
    }

    void runSync();

    window.addEventListener("focus", runSync);
    return () => window.removeEventListener("focus", runSync);
  }, [status]);

  if (!hydrated) {
    return null;
  }

  if (items.length === 0) {
    return (
      <EmptyState
        icon={
          <ShoppingBag aria-hidden className="size-5.5 text-muted-foreground" />
        }
        heading="Your bag is empty"
        description="Nothing here yet. Have a dig through this week's cover story or browse the full catalogue."
      >
        <Link
          href="/"
          className={cn(
            buttonVariants(),
            "px-7.5 py-3.5 text-xs font-bold tracking-wide uppercase",
          )}
        >
          Start browsing
        </Link>
      </EmptyState>
    );
  }

  const subtotalInPence = items.reduce(
    (total, item) => total + item.priceInPence * item.quantity,
    0,
  );

  return (
    <div className="mx-auto w-full max-w-275 px-16 py-11">
      <div className="mb-8 flex items-baseline justify-between">
        <h1 className="font-serif text-4xl font-medium italic">Your bag</h1>
        <span className="text-sm font-semibold text-muted-foreground">
          {totalQuantity} {totalQuantity === 1 ? "item" : "items"}
        </span>
      </div>

      <div className="grid grid-cols-[1.5fr_1fr] items-start gap-14">
        <div>
          <div className="border-t border-line">
            {items.map((item) => (
              <CartRow key={item.albumSqid} item={item} />
            ))}
          </div>
          <Link
            href="/"
            className="mt-7 inline-block border-b-2 border-ink pb-0.5 text-xs font-bold tracking-wide text-ink uppercase"
          >
            ← Continue shopping
          </Link>
        </div>

        <Card className="border-line">
          <CardHeader>
            <CardTitle className="font-serif text-xl font-medium italic">
              Order summary
            </CardTitle>
          </CardHeader>
          <CardContent className="gap-2.5">
            <div className="flex justify-between text-sm text-muted-foreground">
              <span>Subtotal</span>
              <span>{formatPrice(subtotalInPence)}</span>
            </div>
            <div className="flex justify-between text-sm text-muted-foreground">
              <span>Shipping</span>
              <span>Not applicable (demo order)</span>
            </div>
            <div className="mt-2 flex justify-between border-t border-line pt-4 text-base font-bold">
              <span>Total</span>
              <span>{formatPrice(subtotalInPence)}</span>
            </div>
          </CardContent>
          <CardFooter className="flex flex-col gap-3.5">
            <Link
              href="/checkout"
              className={cn(
                buttonVariants(),
                "w-full py-4 text-xs font-bold tracking-wide uppercase",
              )}
            >
              Checkout
            </Link>
            {status !== "authenticated" && (
              <p className="text-center text-[0.6875rem] text-muted-foreground">
                You&apos;ll need to log in to complete checkout
              </p>
            )}
          </CardFooter>
        </Card>
      </div>
    </div>
  );
}
