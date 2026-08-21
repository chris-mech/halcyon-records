"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import { CheckCircle2 } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { buttonVariants } from "@/components/ui/button";
import { formatPrice } from "@/lib/format";
import { cn } from "@/lib/utils";

import type { components } from "@/lib/api/schema";

type OrderDetail = components["schemas"]["OrderDetailResponse"];

interface OrderConfirmationProps {
  orderNumber: string;
}

type FetchState =
  | { status: "loading" }
  | { status: "not-found" }
  | { status: "error" }
  | { status: "loaded"; order: OrderDetail };

function OrderConfirmation({ orderNumber }: OrderConfirmationProps) {
  const [state, setState] = useState<FetchState>({ status: "loading" });

  useEffect(() => {
    let cancelled = false;

    fetch(`/api/orders/${encodeURIComponent(orderNumber)}`)
      .then(async (response) => {
        if (cancelled) return;

        if (response.status === 404) {
          setState({ status: "not-found" });
          return;
        }

        if (!response.ok) {
          setState({ status: "error" });
          return;
        }

        const order: OrderDetail = await response.json();
        setState({ status: "loaded", order });
      })
      .catch(() => {
        if (!cancelled) setState({ status: "error" });
      });

    return () => {
      cancelled = true;
    };
  }, [orderNumber]);

  if (state.status === "loading") {
    return null;
  }

  if (state.status === "not-found") {
    return (
      <div className="mx-auto w-full max-w-105 px-16 py-16 text-center">
        <p className="text-sm text-muted-foreground">
          We couldn&apos;t find that order.
        </p>
      </div>
    );
  }

  if (state.status === "error") {
    return (
      <div className="mx-auto w-full max-w-105 px-16 py-16 text-center">
        <p className="text-sm text-muted-foreground">
          Something went wrong loading your order. Please try again.
        </p>
      </div>
    );
  }

  const { order } = state;

  return (
    <div className="mx-auto w-full max-w-160 px-16 py-16 text-center">
      <div className="mx-auto mb-6 flex size-13 items-center justify-center rounded-full border border-line">
        <CheckCircle2 aria-hidden className="size-6 text-rust" />
      </div>
      <h1 className="mb-2 font-serif text-2xl font-medium italic">
        Order confirmed
      </h1>
      <p className="mb-8 text-sm text-muted-foreground">
        Order {order.orderNumber} · placed{" "}
        {new Date(order.placedAt).toLocaleDateString("en-GB", {
          day: "numeric",
          month: "long",
          year: "numeric",
        })}
      </p>

      <Card className="mb-8 border-line text-left">
        <CardHeader>
          <CardTitle className="font-serif text-xl font-medium italic">
            Order summary
          </CardTitle>
        </CardHeader>
        <CardContent className="gap-3.5">
          {order.items.map((item) => (
            <div key={item.albumSqid} className="flex items-center gap-3">
              <div className="relative size-12 shrink-0 bg-slate-muted/40">
                {item.imageUrl && (
                  <Image
                    src={item.imageUrl}
                    alt=""
                    fill
                    sizes="48px"
                    className="object-cover"
                  />
                )}
              </div>
              <div className="flex-1">
                <p className="text-sm font-medium">{item.title}</p>
                <p className="text-xs text-muted-foreground">
                  Qty {item.quantity}
                </p>
              </div>
              <p className="text-sm font-semibold">
                {formatPrice(item.priceAtPurchaseInPence * item.quantity)}
              </p>
            </div>
          ))}
          <div className="mt-2 flex justify-between border-t border-line pt-4 text-base font-bold">
            <span>Total</span>
            <span>{formatPrice(order.totalInPence)}</span>
          </div>
        </CardContent>
      </Card>

      <div className="border-l-2 border-slate bg-background p-3 text-left text-[0.6875rem] leading-relaxed text-muted-foreground">
        No confirmation email is sent and nothing ships — this page exists to
        show the order flow end to end. The order above has been saved to the
        database, so it&apos;s a genuine record, just not a genuine purchase.
      </div>

      <Link
        href="/shop"
        className={cn(
          buttonVariants(),
          "mt-8 w-full py-4 text-xs font-bold tracking-wide uppercase",
        )}
      >
        Continue shopping
      </Link>
    </div>
  );
}

export { OrderConfirmation };
