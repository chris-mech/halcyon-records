import Link from "next/link";
import { cva, type VariantProps } from "class-variance-authority";

import { cn } from "@/lib/utils";

const wordmarkVariants = cva("font-serif font-semibold text-gold", {
  variants: {
    variant: {
      header: "text-xl text-shadow-[0.1em_0.1em_0_var(--color-ink)]",
      footer: "text-sm",
    },
  },
  defaultVariants: {
    variant: "header",
  },
});

interface WordmarkProps extends VariantProps<typeof wordmarkVariants> {
  className?: string;
}

function Wordmark({ variant = "header", className }: WordmarkProps) {
  return (
    <Link href="/" data-slot="wordmark" className={cn("flex flex-col", className)}>
      <span className={cn(wordmarkVariants({ variant }))}>Halcyon Records</span>
      {variant === "header" && (
        <span className="mt-1 text-[0.5625rem] font-semibold tracking-[0.2em] text-slate-muted uppercase">
          Tagline TBD
        </span>
      )}
    </Link>
  );
}

export { Wordmark };