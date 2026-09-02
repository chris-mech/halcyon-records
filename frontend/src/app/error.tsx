"use client";

import { useEffect } from "react";
import Link from "next/link";

import { Button, buttonVariants } from "@/components/ui/button";
import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { cn } from "@/lib/utils";

function isBackendUnavailableCause(
  cause: unknown,
): cause is { status: number } {
  return (
    typeof cause === "object" &&
    cause !== null &&
    "status" in cause &&
    cause.status === 503
  );
}

export default function Error({
  error,
  retry,
}: {
  error: Error & { digest?: string };
  retry: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  const backendUnavailable = isBackendUnavailableCause(error.cause);

  return (
    <>
      <Header />
      <main className="flex flex-1 flex-col">
        <div className="mx-auto w-full max-w-160 px-16 pt-30 pb-35 text-center">
          <h1 className="mb-3.5 font-serif text-[1.625rem] font-medium italic">
            {backendUnavailable
              ? "The store is taking a moment"
              : "Something broke"}
          </h1>
          <p className="mx-auto mb-10 max-w-100 text-sm leading-relaxed text-muted-foreground">
            {backendUnavailable
              ? "The backend appears to be unavailable right now. This can happen after it's been idle for a while. Please try again shortly."
              : "That wasn't supposed to happen. The page hit an error rendering. Trying again usually fixes it."}
          </p>
          <div className="flex justify-center gap-3.5">
            <Button
              type="button"
              onClick={retry}
              className="h-auto px-7.5 py-3.5 text-[0.8125rem] font-bold tracking-wide uppercase"
            >
              Try again
            </Button>
            <Link
              href="/"
              className={cn(
                buttonVariants({ variant: "outline" }),
                "h-auto border-ink px-7.5 py-3.5 text-[0.8125rem] font-bold tracking-wide text-ink uppercase",
              )}
            >
              Go home
            </Link>
          </div>
        </div>
      </main>
      <Footer />
    </>
  );
}
