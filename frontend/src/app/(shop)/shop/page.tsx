import { Suspense } from "react";
import type { Metadata } from "next";

import { client } from "@/lib/api/client";
import { LoadingState } from "@/components/loading-state";
import { SkeletonCardGrid } from "@/components/skeleton-primitives";
import { Skeleton } from "@/components/ui/skeleton";
import { ProductCard } from "@/components/product-card";
import { ShadowStackText } from "@/components/shadow-stack-text";

import { FilterPills } from "./filter-pills";
import { Pagination } from "./pagination";
import { PAGE_SIZE, parseShopFilters } from "./search-params";
import { SortSelect } from "./sort-select";

export const metadata: Metadata = {
  title: "Shop",
  description:
    "Browse the full record catalogue and filter by genre, price, and more.",
};

export async function ShopContent({
  searchParams,
}: Pick<PageProps<"/shop">, "searchParams">) {
  const filters = parseShopFilters(await searchParams);

  const { data, error } = await client.GET("/api/albums", {
    params: {
      query: {
        page: filters.page,
        pageSize: PAGE_SIZE,
        isNew: filters.isNew,
        isOnSale: filters.isOnSale,
        isStaffPick: filters.isStaffPick,
        genres: filters.genres,
        sort: filters.sort,
      },
    },
  });

  if (error) {
    throw new Error("Failed to load albums.");
  }

  const totalPages = data.totalPages ?? Math.ceil(data.totalCount / PAGE_SIZE);

  return (
    <div className="flex flex-col gap-8 px-16 py-12">
      <div className="flex items-baseline justify-between">
        <ShadowStackText as="h1" size="section">
          All records
        </ShadowStackText>
        <span className="text-sm text-muted-foreground">
          {data.totalCount} {data.totalCount === 1 ? "record" : "records"}
        </span>
      </div>

      <div className="flex flex-wrap items-center justify-between gap-4">
        <FilterPills filters={filters} />
        <SortSelect filters={filters} />
      </div>

      {data.items.length === 0 ? (
        <p className="text-sm text-muted-foreground">
          No records match these filters.
        </p>
      ) : (
        <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-4">
          {data.items.map((album) => (
            <ProductCard key={album.sqid} album={album} />
          ))}
        </div>
      )}

      <Pagination filters={filters} totalPages={totalPages} />
    </div>
  );
}

function ShopSkeleton() {
  return (
    <LoadingState>
      <div className="px-16 pt-12 pb-8">
        <Skeleton className="h-8 w-48" />
      </div>
      <div className="px-16 pb-12">
        <SkeletonCardGrid />
      </div>
    </LoadingState>
  );
}

export default function ShopPage(props: PageProps<"/shop">) {
  return (
    <Suspense fallback={<ShopSkeleton />}>
      <ShopContent searchParams={props.searchParams} />
    </Suspense>
  );
}
