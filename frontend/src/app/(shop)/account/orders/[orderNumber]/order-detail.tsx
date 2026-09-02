"use client";

import { Fragment, useEffect, useState } from "react";
import Link from "next/link";

import { LoadingState } from "@/components/loading-state";
import { SkeletonLines } from "@/components/skeleton-primitives";
import { Skeleton } from "@/components/ui/skeleton";
import { MediaThumbnail } from "@/components/media-thumbnail";
import { Badge } from "@/components/ui/badge";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
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
      <LoadingState>
        <div className="mx-auto w-full max-w-225 px-16 py-11">
          <Skeleton className="mb-6 h-4 w-56" />
          <div className="mb-9 flex items-baseline justify-between gap-6 border-b border-line pb-7">
            <div className="flex flex-col gap-2">
              <Skeleton className="h-9 w-48" />
              <Skeleton className="h-3 w-40" />
            </div>
            <Skeleton className="h-6 w-24" />
          </div>
          <div className="grid grid-cols-[1.4fr_1fr] items-start gap-12">
            <SkeletonLines count={4} />
            <SkeletonLines count={3} />
          </div>
        </div>
      </LoadingState>
    );
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
    <div className="mx-auto w-full max-w-225 px-16 py-11">
      <Breadcrumb className="mb-6">
        <BreadcrumbList className="gap-2 text-xs font-semibold text-muted-foreground sm:gap-2">
          <BreadcrumbItem>
            <BreadcrumbLink
              render={<Link href="/">Home</Link>}
              className="hover:text-ink"
            />
          </BreadcrumbItem>
          <BreadcrumbSeparator className="text-line">/</BreadcrumbSeparator>
          <BreadcrumbItem>
            <BreadcrumbLink
              render={<Link href="/account">Account</Link>}
              className="hover:text-ink"
            />
          </BreadcrumbItem>
          <BreadcrumbSeparator className="text-line">/</BreadcrumbSeparator>
          <BreadcrumbItem>
            <BreadcrumbLink
              render={<Link href="/account">Order history</Link>}
              className="hover:text-ink"
            />
          </BreadcrumbItem>
          <BreadcrumbSeparator className="text-line">/</BreadcrumbSeparator>
          <BreadcrumbItem>
            <BreadcrumbPage className="text-ink">
              {order.orderNumber}
            </BreadcrumbPage>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>

      <div className="mb-9 flex items-baseline justify-between gap-6 border-b border-line pb-7">
        <div>
          <h1 className="mb-2 font-serif text-3xl font-medium italic">
            Order {order.orderNumber}
          </h1>
          <p className="text-[0.8125rem] font-semibold text-muted-foreground">
            Placed{" "}
            {new Date(order.placedAt).toLocaleDateString("en-GB", {
              day: "numeric",
              month: "long",
              year: "numeric",
            })}
          </p>
        </div>
        <Badge
          variant="outline"
          className="h-auto shrink-0 rounded-none border-line px-3.5 py-1.5 text-[11px] font-bold tracking-wide text-slate uppercase"
        >
          {order.status}
        </Badge>
      </div>

      <div className="grid grid-cols-[1.4fr_1fr] items-start gap-12">
        <div>
          <div className="border-t border-line">
            {order.items.map((item) => (
              <div
                key={item.albumSqid}
                className="flex items-center gap-4.5 border-b border-line py-5"
              >
                <MediaThumbnail
                  imageUrl={item.imageUrl}
                  sizes="64px"
                  className="size-16 shrink-0"
                />
                <div className="flex-1">
                  <p className="mb-1 flex flex-wrap gap-x-1 text-[0.6875rem] font-bold tracking-wide text-muted-foreground uppercase">
                    {item.artists.map((artist, index) => (
                      <Fragment key={artist.sqid}>
                        {index > 0 && ", "}
                        <Link
                          href={`/artists/${artist.sqid}/${artist.nameSlug}`}
                          className="hover:underline"
                        >
                          {artist.name}
                        </Link>
                      </Fragment>
                    ))}
                  </p>
                  <p className="font-serif text-base italic">{item.title}</p>
                  <p className="mt-1 text-xs text-muted-foreground">
                    Qty {item.quantity}
                  </p>
                </div>
                <p className="text-sm font-bold">
                  {formatPrice(item.priceAtPurchaseInPence * item.quantity)}
                </p>
              </div>
            ))}
          </div>

          <Link
            href="/account"
            className="mt-7 inline-block border-b-2 border-ink pb-0.5 text-xs font-bold tracking-wide text-ink uppercase"
          >
            ← Back to order history
          </Link>
        </div>

        <Card className="border-line">
          <CardHeader>
            <CardTitle className="font-serif text-lg font-medium italic">
              Order summary
            </CardTitle>
          </CardHeader>
          <CardContent className="gap-0">
            <div className="flex justify-between border-t border-line pt-4.5 text-base font-bold">
              <span>Total</span>
              <span>{formatPrice(order.totalInPence)}</span>
            </div>

            <div className="mt-6 border-l-2 border-slate bg-background p-3 text-[0.6875rem] leading-relaxed text-muted-foreground">
              This is a demo order. No payment was collected and nothing was
              shipped.
            </div>

            <div className="mt-6 border-t border-line pt-6">
              <p className="mb-2 text-xs font-bold tracking-wide text-muted-foreground uppercase">
                Contact details
              </p>
              <p className="text-sm">
                {order.contactFirstName} {order.contactLastName}
              </p>
              <p className="text-sm text-muted-foreground">
                {order.contactEmail}
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

export { OrderDetail };
