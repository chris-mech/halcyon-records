import Link from "next/link";
import { Lock } from "lucide-react";

import { buttonVariants } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { cn } from "@/lib/utils";

function CheckoutGate() {
  return (
    <div className="mx-auto max-w-105 px-16 py-16">
      <Card className="border-line text-center">
        <CardHeader>
          <div className="mx-auto mb-2 flex size-13 items-center justify-center rounded-full border border-line">
            <Lock aria-hidden className="size-5 text-slate" />
          </div>
          <CardTitle className="text-center font-serif text-2xl font-medium italic">
            Log in to check out
          </CardTitle>
          <CardDescription className="text-center text-sm leading-relaxed">
            An account keeps your order somewhere you can find it again —
            you&apos;ll be able to see it under &quot;Order history&quot;
            afterward.
          </CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col gap-3">
          <Link
            href="/login?next=/checkout"
            className={cn(
              buttonVariants(),
              "w-full py-3.5 text-xs font-bold tracking-wide uppercase",
            )}
          >
            Log in
          </Link>
          <Link
            href="/register?next=/checkout"
            className={cn(
              buttonVariants({ variant: "outline" }),
              "w-full py-3.5 text-xs font-bold tracking-wide uppercase",
            )}
          >
            Create an account
          </Link>
        </CardContent>
        <CardFooter className="justify-center">
          <p className="text-center text-xs text-muted-foreground">
            Your bag is saved — nothing will be lost.
          </p>
        </CardFooter>
      </Card>
    </div>
  );
}

export { CheckoutGate };
