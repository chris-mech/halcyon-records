"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import { PackageOpen } from "lucide-react";

import { EmptyState } from "@/components/empty-state";
import { buttonVariants } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { formatPrice } from "@/lib/format";
import { cn } from "@/lib/utils";

import { OrdersPagination } from "./pagination";

import type { components } from "@/lib/api/schema";

type PagedOrders = components["schemas"]["PagedResultOfOrderSummaryResponse"];

interface OrderHistoryProps {
  page: number;
}

type FetchState =
  | { status: "loading" }
  | { status: "error" }
  | { status: "loaded"; result: PagedOrders };

const PAGE_SIZE = 10;

function OrderHistory({ page }: OrderHistoryProps) {
  const [state, setState] = useState<FetchState>({ status: "loading" });

  useEffect(() => {
    let cancelled = false;
    setState({ status: "loading" });

    fetch(`/api/orders?page=${page}&pageSize=${PAGE_SIZE}`)
      .then(async (response) => {
        if (cancelled) return;

        if (!response.ok) {
          setState({ status: "error" });
          return;
        }

        const result: PagedOrders = await response.json();
        setState({ status: "loaded", result });
      })
      .catch(() => {
        if (!cancelled) setState({ status: "error" });
      });

    return () => {
      cancelled = true;
    };
  }, [page]);

  if (state.status === "loading") {
    return (
      <div className="flex flex-1 items-center justify-center px-16 py-20 text-sm text-muted-foreground">
        Loading…
      </div>
    );
  }

  if (state.status === "error") {
    return (
      <div className="mx-auto max-w-105 px-16 py-16 text-center">
        <p className="text-sm text-muted-foreground">
          Something went wrong loading your orders. Please try again.
        </p>
      </div>
    );
  }

  const { result } = state;
  const totalPages =
    result.totalPages ?? Math.ceil(result.totalCount / PAGE_SIZE);

  if (result.items.length === 0) {
    return (
      <EmptyState
        icon={
          <PackageOpen aria-hidden className="size-5.5 text-muted-foreground" />
        }
        heading="You haven't placed any orders yet"
        description="Everything you buy will show up here, so you can find it again later."
      >
        <Link
          href="/shop"
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
    <div className="mx-auto max-w-275 px-16 py-11">
      <h1 className="mb-8 font-serif text-4xl font-medium italic">
        Order history
      </h1>

      <div className="mb-8 flex flex-col gap-4">
        {result.items.map((order) => (
          <Link
            key={order.orderNumber}
            href={`/account/orders/${order.orderNumber}`}
          >
            <Card className="border-line transition-colors hover:border-ink">
              <CardHeader>
                <CardTitle className="font-serif text-lg font-medium italic">
                  Order {order.orderNumber}
                </CardTitle>
                <CardDescription>
                  {order.status} · placed{" "}
                  {new Date(order.placedAt).toLocaleDateString("en-GB", {
                    day: "numeric",
                    month: "long",
                    year: "numeric",
                  })}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="flex items-center gap-3">
                  <div className="flex -space-x-3">
                    {order.items.slice(0, 4).map((item) => (
                      <div
                        key={item.albumSqid}
                        className="relative size-12 shrink-0 border border-paper bg-slate-muted/40"
                      >
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
                    ))}
                  </div>
                  <div className="flex-1 text-sm text-muted-foreground">
                    {order.items.length}{" "}
                    {order.items.length === 1 ? "item" : "items"}
                  </div>
                  <p className="text-sm font-semibold">
                    {formatPrice(order.totalInPence)}
                  </p>
                </div>
              </CardContent>
            </Card>
          </Link>
        ))}
      </div>

      <OrdersPagination page={page} totalPages={totalPages} />
    </div>
  );
}

export { OrderHistory };
