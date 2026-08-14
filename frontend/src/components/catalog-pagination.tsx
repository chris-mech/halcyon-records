import Link from "next/link";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { getPageNumbers } from "@/lib/pagination";
import {
  buildCatalogHref,
  type CatalogFilters,
} from "@/lib/catalog-search-params";

interface CatalogPaginationProps {
  basePath: string;
  filters: CatalogFilters;
  totalPages: number;
}

const pagerTextClassName = "text-[0.8125rem] font-semibold";
const activePageClassName =
  "border-ink bg-ink text-paper hover:bg-ink hover:text-paper";
const inactivePageClassName = "bg-paper text-ink";

function CatalogPagination({
  basePath,
  filters,
  totalPages,
}: CatalogPaginationProps) {
  if (totalPages <= 1) {
    return null;
  }

  const current = filters.page;

  return (
    <nav
      aria-label="Pagination"
      className="flex flex-wrap items-center gap-2.5"
    >
      {current > 1 && (
        <Link
          href={buildCatalogHref(basePath, filters, { page: current - 1 })}
          className={cn(
            buttonVariants({ variant: "outline" }),
            pagerTextClassName,
            "bg-paper px-4 text-ink",
          )}
        >
          ← Prev
        </Link>
      )}
      {getPageNumbers(current, totalPages).map((page, index) =>
        page === "ellipsis" ? (
          <span
            key={`ellipsis-${index}`}
            className={cn(pagerTextClassName, "px-1 text-muted-foreground")}
          >
            …
          </span>
        ) : (
          <Link
            key={page}
            href={buildCatalogHref(basePath, filters, { page })}
            aria-current={page === current ? "page" : undefined}
            className={cn(
              buttonVariants({ variant: "outline", size: "icon" }),
              pagerTextClassName,
              page === current ? activePageClassName : inactivePageClassName,
            )}
          >
            {page}
          </Link>
        ),
      )}
      {current < totalPages && (
        <Link
          href={buildCatalogHref(basePath, filters, { page: current + 1 })}
          className={cn(
            buttonVariants({ variant: "outline" }),
            pagerTextClassName,
            "bg-paper px-4 text-ink",
          )}
        >
          Next →
        </Link>
      )}
    </nav>
  );
}

export { CatalogPagination };
