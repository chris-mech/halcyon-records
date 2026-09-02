import { Suspense } from "react";
import type { Metadata } from "next";
import { redirect } from "next/navigation";

import { auth } from "@/auth";
import { LoadingState } from "@/components/loading-state";
import { SkeletonLines } from "@/components/skeleton-primitives";
import { Skeleton } from "@/components/ui/skeleton";

import { OrderDetail } from "./order-detail";

export async function generateMetadata({
  params,
}: Pick<
  PageProps<"/account/orders/[orderNumber]">,
  "params"
>): Promise<Metadata> {
  const { orderNumber } = await params;

  return {
    title: `Order ${orderNumber}`,
  };
}

export async function OrderDetailGate({
  params,
}: Pick<PageProps<"/account/orders/[orderNumber]">, "params">) {
  const { orderNumber } = await params;
  const session = await auth();

  if (!session) {
    redirect(`/login?next=/account/orders/${orderNumber}`);
  }

  return <OrderDetail orderNumber={orderNumber} />;
}

function OrderDetailSkeleton() {
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

export default function OrderDetailPage(
  props: PageProps<"/account/orders/[orderNumber]">,
) {
  return (
    <Suspense fallback={<OrderDetailSkeleton />}>
      <OrderDetailGate params={props.params} />
    </Suspense>
  );
}
