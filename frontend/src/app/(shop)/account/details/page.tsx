import { Suspense } from "react";
import { redirect } from "next/navigation";

import { auth } from "@/auth";

import { AccountDetails } from "./account-details";
import { AccountShell } from "../account-shell";

export async function AccountDetailsContent() {
  const session = await auth();

  if (!session) {
    redirect("/login?next=/account/details");
  }

  return (
    <AccountShell active="details">
      <AccountDetails />
    </AccountShell>
  );
}

function AccountDetailsSkeleton() {
  return (
    <div className="flex flex-1 items-center justify-center px-16 py-20 text-sm text-muted-foreground">
      Loading…
    </div>
  );
}

export default function AccountDetailsPage() {
  return (
    <Suspense fallback={<AccountDetailsSkeleton />}>
      <AccountDetailsContent />
    </Suspense>
  );
}
