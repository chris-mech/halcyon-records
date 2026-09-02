"use client";

import { useEffect, useState } from "react";

import { LoadingState } from "@/components/loading-state";
import { Skeleton } from "@/components/ui/skeleton";
import { Card, CardContent } from "@/components/ui/card";

import type { components } from "@/lib/api/schema";

type CurrentUser = components["schemas"]["CurrentUserResponse"];

type FetchState =
  | { status: "loading" }
  | { status: "error" }
  | { status: "loaded"; user: CurrentUser };

function AccountDetails() {
  const [state, setState] = useState<FetchState>({ status: "loading" });

  useEffect(() => {
    let cancelled = false;

    fetch("/api/auth/me")
      .then(async (response) => {
        if (cancelled) return;

        if (!response.ok) {
          setState({ status: "error" });
          return;
        }

        const user: CurrentUser = await response.json();
        setState({ status: "loaded", user });
      })
      .catch(() => {
        if (!cancelled) setState({ status: "error" });
      });

    return () => {
      cancelled = true;
    };
  }, []);

  if (state.status === "loading") {
    return (
      <LoadingState>
        <Card className="border-line">
          <CardContent className="gap-0 divide-y divide-line px-7">
            {Array.from({ length: 3 }, (_, index) => (
              <div key={index} className="flex justify-between py-4.5">
                <Skeleton className="h-3 w-20" />
                <Skeleton className="h-4 w-32" />
              </div>
            ))}
          </CardContent>
        </Card>
      </LoadingState>
    );
  }

  if (state.status === "error") {
    return (
      <div className="py-16 text-center text-sm text-muted-foreground">
        Something went wrong loading your account details. Please try again.
      </div>
    );
  }

  const { user } = state;

  return (
    <div>
      <Card className="border-line">
        <CardContent className="gap-0 divide-y divide-line px-7">
          <div className="flex justify-between py-4.5">
            <span className="text-xs font-bold tracking-wide text-muted-foreground uppercase">
              Name
            </span>
            <span className="text-sm font-semibold">
              {user.firstName} {user.lastName}
            </span>
          </div>
          <div className="flex justify-between py-4.5">
            <span className="text-xs font-bold tracking-wide text-muted-foreground uppercase">
              Email
            </span>
            <span className="text-sm font-semibold">{user.email}</span>
          </div>
          <div className="flex justify-between py-4.5">
            <span className="text-xs font-bold tracking-wide text-muted-foreground uppercase">
              Member since
            </span>
            <span className="text-sm font-semibold">
              {new Date(user.registeredAt).toLocaleDateString("en-GB", {
                day: "numeric",
                month: "long",
                year: "numeric",
              })}
            </span>
          </div>
        </CardContent>
      </Card>

      <div className="mt-5 border-l-2 border-slate bg-background p-3 text-[0.6875rem] leading-relaxed text-muted-foreground">
        Editing your details isn&apos;t available in this demo yet. This view is
        read-only for now.
      </div>
    </div>
  );
}

export { AccountDetails };
