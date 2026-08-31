import { Suspense } from "react";
import type { Metadata } from "next";
import { redirect } from "next/navigation";

import { auth } from "@/auth";
import { LoadingState } from "@/components/loading-state";
import { SkeletonLines } from "@/components/skeleton-primitives";
import { Skeleton } from "@/components/ui/skeleton";

import { AccountDetails } from "./account-details";
import { AccountShell } from "../account-shell";

export const metadata: Metadata = {
  title: "Account Details",
  description: "Your Halcyon Records account details.",
};

export async function AccountDetailsContent() {
  const session = await auth();

  if (!session || session.error) {
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
    <LoadingState>
      <div className="mx-auto w-full max-w-275 px-16 py-11">
        <Skeleton className="mb-6 h-4 w-40" />
        <Skeleton className="mb-1.5 h-10 w-56" />
        <Skeleton className="mb-8 h-3 w-72" />
        <div className="grid grid-cols-[220px_1fr] items-start gap-12">
          <div className="flex flex-col gap-4 border-r border-line pr-4">
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-4 w-32" />
          </div>
          <SkeletonLines count={3} />
        </div>
      </div>
    </LoadingState>
  );
}

export default function AccountDetailsPage() {
  return (
    <Suspense fallback={<AccountDetailsSkeleton />}>
      <AccountDetailsContent />
    </Suspense>
  );
}
