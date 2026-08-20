import Link from "next/link";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { getPageNumbers } from "@/lib/pagination";

interface OrdersPaginationProps {
  page: number;
  totalPages: number;
}

const pagerTextClassName = "text-[0.8125rem] font-semibold";
const activePageClassName =
  "border-ink bg-ink text-paper hover:bg-ink hover:text-paper";
const inactivePageClassName = "bg-paper text-ink";

function OrdersPagination({ page, totalPages }: OrdersPaginationProps) {
  if (totalPages <= 1) {
    return null;
  }

  return (
    <nav
      aria-label="Pagination"
      className="flex flex-wrap items-center gap-2.5"
    >
      {page > 1 && (
        <Link
          href={`/account?page=${page - 1}`}
          className={cn(
            buttonVariants({ variant: "outline" }),
            pagerTextClassName,
            "bg-paper px-4 text-ink",
          )}
        >
          ← Prev
        </Link>
      )}
      {getPageNumbers(page, totalPages).map((pageNumber, index) =>
        pageNumber === "ellipsis" ? (
          <span
            key={`ellipsis-${index}`}
            className={cn(pagerTextClassName, "px-1 text-muted-foreground")}
          >
            …
          </span>
        ) : (
          <Link
            key={pageNumber}
            href={`/account?page=${pageNumber}`}
            aria-current={pageNumber === page ? "page" : undefined}
            className={cn(
              buttonVariants({ variant: "outline", size: "icon" }),
              pagerTextClassName,
              pageNumber === page ? activePageClassName : inactivePageClassName,
            )}
          >
            {pageNumber}
          </Link>
        ),
      )}
      {page < totalPages && (
        <Link
          href={`/account?page=${page + 1}`}
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

export { OrdersPagination };
