import Link from "next/link";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

import { buildShopHref, type ShopFilters } from "./search-params";

interface FilterPillsProps {
  filters: ShopFilters;
}

const TOGGLE_PILLS = [
  { key: "isNew", label: "New in" },
  { key: "isOnSale", label: "On sale" },
  { key: "isStaffPick", label: "Staff picks" },
] as const;

const pillClassName =
  "h-auto px-4.5 py-2.25 text-xs font-semibold tracking-wide uppercase";
const activeClassName =
  "border-ink bg-ink text-paper hover:bg-ink hover:text-paper";
const inactiveClassName = "border-ink bg-transparent text-ink";

function FilterPills({ filters }: FilterPillsProps) {
  const isAllActive =
    !filters.isNew && !filters.isOnSale && !filters.isStaffPick;

  return (
    <div className="flex flex-wrap gap-2.5">
      <Link
        href={buildShopHref(filters, {
          isNew: false,
          isOnSale: false,
          isStaffPick: false,
        })}
        aria-current={isAllActive ? "true" : undefined}
        className={cn(
          buttonVariants({ variant: "outline" }),
          pillClassName,
          isAllActive ? activeClassName : inactiveClassName,
        )}
      >
        All
      </Link>
      {TOGGLE_PILLS.map(({ key, label }) => {
        const active = filters[key];
        return (
          <Link
            key={key}
            href={buildShopHref(filters, { [key]: !active })}
            aria-current={active ? "true" : undefined}
            className={cn(
              buttonVariants({ variant: "outline" }),
              pillClassName,
              active ? activeClassName : inactiveClassName,
            )}
          >
            {label}
          </Link>
        );
      })}
    </div>
  );
}

export { FilterPills };
