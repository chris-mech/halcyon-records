import { Suspense } from "react";
import { redirect } from "next/navigation";

import { auth } from "@/auth";

import { OrderHistory } from "./order-history";

export async function AccountContent({
  searchParams,
}: Pick<PageProps<"/account">, "searchParams">) {
  const { page } = await searchParams;
  const session = await auth();

  if (!session) {
    redirect(
      page ? `/login?next=/account?page=${page}` : "/login?next=/account",
    );
  }

  return <OrderHistory page={Number(page) || 1} />;
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
