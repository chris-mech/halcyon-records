"use client";

import Link from "next/link";
import { ShoppingCart } from "lucide-react";
import { useSession } from "next-auth/react";

import { EmptyState } from "@/components/empty-state";
import { StepIndicator } from "@/components/step-indicator";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { useCartHydrated, useCartStore } from "@/lib/cart/cart-store";

import { CheckoutForm } from "./checkout-form";
import { CheckoutGate } from "./checkout-gate";

export default function CheckoutPage() {
  const hydrated = useCartHydrated();
  const items = useCartStore((state) => state.items);
  const { status } = useSession();

  if (!hydrated || status === "loading") {
    return null;
  }

  if (items.length === 0) {
    return (
      <EmptyState
        icon={
          <ShoppingCart
            aria-hidden
            className="size-5.5 text-muted-foreground"
          />
        }
        heading="Your cart is empty"
        description="Add something to your cart before checking out."
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

  return (
    <>
      <div className="mx-auto w-full max-w-275 px-16 pt-9">
        <StepIndicator
          currentStep={status === "authenticated" ? "checkout" : "login"}
        />
      </div>
      {status === "authenticated" ? <CheckoutForm /> : <CheckoutGate />}
    </>
  );
}
