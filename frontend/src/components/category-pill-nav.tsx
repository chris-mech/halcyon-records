import Link from "next/link";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

interface CategoryPillNavItem {
  key: string;
  label: string;
  href: string;
}

interface CategoryPillNavProps {
  ariaLabel: string;
  activeKey: string;
  items: CategoryPillNavItem[];
}

function CategoryPillNav({
  ariaLabel,
  activeKey,
  items,
}: CategoryPillNavProps) {
  return (
    <nav aria-label={ariaLabel} className="flex flex-wrap gap-2.5">
      {items.map((item) => (
        <Link
          key={item.key}
          href={item.href}
          aria-current={item.key === activeKey ? "page" : undefined}
          className={cn(
            buttonVariants({ variant: "outline" }),
            "h-auto w-fit border-ink px-4.5 py-2.25 text-xs font-semibold uppercase",
            item.key === activeKey &&
              "bg-ink text-paper hover:bg-ink hover:text-paper",
          )}
        >
          {item.label}
        </Link>
      ))}
    </nav>
  );
}

export { CategoryPillNav };
