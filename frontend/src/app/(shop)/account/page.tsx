import { Suspense } from "react";
import type { Metadata } from "next";
import { redirect } from "next/navigation";

import { auth } from "@/auth";

import { OrderHistory } from "./order-history";
import { AccountShell } from "./account-shell";

export const metadata: Metadata = {
  title: "Order History",
  description: "Your past Halcyon Records orders.",
};

export async function AccountContent({
  searchParams,
}: Pick<PageProps<"/account">, "searchParams">) {
  const { page } = await searchParams;
  const session = await auth();

  if (!session || session.error) {
    redirect(
      page ? `/login?next=/account?page=${page}` : "/login?next=/account",
    );
  }

  const pageNumber = Number(page) || 1;

  return (
    <AccountShell active="orders">
      <OrderHistory key={pageNumber} page={pageNumber} />
    </AccountShell>
  );
}

function AccountSkeleton() {
  return (
    <div className="flex flex-1 items-center justify-center px-16 py-20 text-sm text-muted-foreground">
      Loading…
    </div>
  );
}

export default function AccountPage(props: PageProps<"/account">) {
  return (
    <Suspense fallback={<AccountSkeleton />}>
      <AccountContent searchParams={props.searchParams} />
    </Suspense>
  );
}
