import { cva, type VariantProps } from "class-variance-authority";

import { cn } from "@/lib/utils";

const shadowStackVariants = cva(
  "font-heading font-black text-paper uppercase",
  {
    variants: {
      size: {
        hero: "text-[4rem] leading-[0.88] shadow-stack-hero",
        section: "text-[2rem] shadow-stack-section",
      },
    },
    defaultVariants: {
      size: "section",
    },
  },
);

interface ShadowStackHeadingProps extends VariantProps<
  typeof shadowStackVariants
> {
  as?: "h1" | "h2";
  className?: string;
  children: React.ReactNode;
}

function ShadowStackHeading({
  as: Tag = "h2",
  size,
  className,
  children,
}: ShadowStackHeadingProps) {
  return (
    <Tag className={cn(shadowStackVariants({ size }), className)}>
      {children}
    </Tag>
  );
}

export { ShadowStackHeading };
