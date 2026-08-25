import { Suspense } from "react";
import type { Metadata } from "next";
import { notFound } from "next/navigation";

import { StepIndicator } from "@/components/step-indicator";

import { OrderConfirmation } from "./order-confirmation";

export async function generateMetadata({
  searchParams,
}: Pick<
  PageProps<"/checkout/confirmation">,
  "searchParams"
>): Promise<Metadata> {
  const { order } = await searchParams;

  return {
    title:
      typeof order === "string" && order.length > 0
        ? `Order ${order} Confirmed`
        : "Order Confirmed",
  };
}

export async function ConfirmationContent({
  searchParams,
}: Pick<PageProps<"/checkout/confirmation">, "searchParams">) {
  const { order } = await searchParams;

  if (typeof order !== "string" || order.length === 0) {
    notFound();
  }

  return (
    <>
      <div className="mx-auto w-full max-w-275 px-16 pt-9">
        <StepIndicator currentStep="confirmation" />
      </div>
      <OrderConfirmation orderNumber={order} />
    </>
  );
}

export default function ConfirmationPage(
  props: PageProps<"/checkout/confirmation">,
) {
  return (
    <Suspense>
      <ConfirmationContent searchParams={props.searchParams} />
    </Suspense>
  );
}
