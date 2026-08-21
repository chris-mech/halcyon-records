"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import { PackageOpen } from "lucide-react";

import { EmptyState } from "@/components/empty-state";
import { buttonVariants } from "@/components/ui/button";
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
    return <div className="py-16 text-sm text-muted-foreground">Loading…</div>;
  }

  if (state.status === "error") {
    return (
      <div className="py-16 text-center text-sm text-muted-foreground">
        Something went wrong loading your orders. Please try again.
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
    <div>
      <div className="border-t border-line">
        {result.items.map((order) => (
          <div key={order.orderNumber} className="border-b border-line py-6">
            <div className="mb-4 flex items-baseline justify-between">
              <span className="text-sm font-bold">
                Order {order.orderNumber}
              </span>
              <span className="text-xs text-muted-foreground">
                {new Date(order.placedAt).toLocaleDateString("en-GB", {
                  day: "numeric",
                  month: "long",
                  year: "numeric",
                })}
              </span>
            </div>

            <div className="mb-4 flex gap-3">
              {order.items.slice(0, 4).map((item) => (
                <div
                  key={item.albumSqid}
                  className="relative size-12 shrink-0 bg-slate-muted/40"
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

            <div className="flex items-center justify-between">
              <span className="text-[0.6875rem] font-bold tracking-wide text-slate uppercase">
                {order.status}
              </span>
              <p className="text-sm font-bold">
                {formatPrice(order.totalInPence)}
              </p>
              <Link
                href={`/account/orders/${order.orderNumber}`}
                className="border-b-2 border-ink pb-0.5 text-xs font-bold tracking-wide text-ink uppercase"
              >
                View order
              </Link>
            </div>
          </div>
        ))}
      </div>

      <div className="pt-8">
        <OrdersPagination page={page} totalPages={totalPages} />
      </div>
    </div>
  );
}

export { OrderHistory };
