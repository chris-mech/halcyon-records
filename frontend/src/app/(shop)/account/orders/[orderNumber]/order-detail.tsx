"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatPrice } from "@/lib/format";

import type { components } from "@/lib/api/schema";

type Order = components["schemas"]["OrderDetailResponse"];

interface OrderDetailProps {
  orderNumber: string;
}

type FetchState =
  | { status: "loading" }
  | { status: "not-found" }
  | { status: "error" }
  | { status: "loaded"; order: Order };

function OrderDetail({ orderNumber }: OrderDetailProps) {
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

        const order: Order = await response.json();
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
    return (
      <div className="flex flex-1 items-center justify-center px-16 py-20 text-sm text-muted-foreground">
        Loading…
      </div>
    );
  }

  if (state.status === "not-found") {
    return (
      <div className="mx-auto max-w-105 px-16 py-16 text-center">
        <p className="text-sm text-muted-foreground">
          We couldn&apos;t find that order.
        </p>
      </div>
    );
  }

  if (state.status === "error") {
    return (
      <div className="mx-auto max-w-105 px-16 py-16 text-center">
        <p className="text-sm text-muted-foreground">
          Something went wrong loading your order. Please try again.
        </p>
      </div>
    );
  }

  const { order } = state;

  return (
    <div className="mx-auto max-w-160 px-16 py-16">
      <Link
        href="/account"
        className="mb-6 inline-block border-b-2 border-ink pb-0.5 text-xs font-bold tracking-wide text-ink uppercase"
      >
        ← Back to order history
      </Link>

      <h1 className="mb-2 font-serif text-2xl font-medium italic">
        Order {order.orderNumber}
      </h1>
      <p className="mb-8 text-sm text-muted-foreground">
        {order.status} · placed{" "}
        {new Date(order.placedAt).toLocaleDateString("en-GB", {
          day: "numeric",
          month: "long",
          year: "numeric",
        })}
      </p>

      <Card className="mb-6 border-line">
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

      <Card className="border-line">
        <CardHeader>
          <CardTitle className="font-serif text-xl font-medium italic">
            Contact details
          </CardTitle>
        </CardHeader>
        <CardContent className="gap-1.5 text-sm text-muted-foreground">
          <p>
            {order.contactFirstName} {order.contactLastName}
          </p>
          <p>{order.contactEmail}</p>
        </CardContent>
      </Card>
    </div>
  );
}

export { OrderDetail };
