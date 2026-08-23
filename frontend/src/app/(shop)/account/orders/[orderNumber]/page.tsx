import { Suspense } from "react";
import type { Metadata } from "next";
import { redirect } from "next/navigation";

import { auth } from "@/auth";

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
    <div className="flex flex-1 items-center justify-center px-16 py-20 text-sm text-muted-foreground">
      Loading…
    </div>
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
