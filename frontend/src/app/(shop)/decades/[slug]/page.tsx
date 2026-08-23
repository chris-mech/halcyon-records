import { Suspense } from "react";
import { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { cacheLife } from "next/cache";

import { PAGE_SIZE, parseCatalogFilters } from "@/lib/catalog-search-params";
import type { components } from "@/lib/api/schema";
import { client } from "@/lib/api/client";
import { ProductCard } from "@/components/product-card";
import { CatalogSortSelect } from "@/components/catalog-sort-select";
import { CatalogPagination } from "@/components/catalog-pagination";
import { CategoryPillNav } from "@/components/category-pill-nav";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";

export async function generateMetadata({
  params,
}: Pick<PageProps<"/decades/[slug]">, "params">): Promise<Metadata> {
  const { slug } = await params;
  const result = await getDecadeDetailData(slug);

  if (!result.found) {
    notFound();
  }

  const { decade } = result;

  return {
    title: decade.label,
    description: decade.description ?? undefined,
    openGraph: decade.imageUrl ? { images: [decade.imageUrl] } : undefined,
  };
}

type DecadeDetail = components["schemas"]["DecadeDetailResponse"];
type DecadeListItem = components["schemas"]["DecadeListItemResponse"];

type DecadeDetailResult =
  { found: true; decade: DecadeDetail } | { found: false };

async function getDecadeDetailData(slug: string): Promise<DecadeDetailResult> {
  "use cache";
  cacheLife({ stale: 60, revalidate: 60, expire: 300 });

  const { data: decade, error } = await client.GET("/api/decades/{slug}", {
    params: { path: { slug } },
  });

  if (error) {
    return { found: false };
  }

  return { found: true, decade };
}

async function getDecadeListData(): Promise<DecadeListItem[]> {
  "use cache";
  cacheLife({ stale: 60, revalidate: 60, expire: 300 });

  const { data, error } = await client.GET("/api/decades");

  return error ? [] : data;
}

export async function DecadeLandingContent({
  params,
  searchParams,
}: Pick<PageProps<"/decades/[slug]">, "params" | "searchParams">) {
  const { slug } = await params;
  const filters = parseCatalogFilters(await searchParams);

  const [detailResult, decades] = await Promise.all([
    getDecadeDetailData(slug),
    getDecadeListData(),
  ]);

  if (!detailResult.found) {
    notFound();
  }

  const { decade } = detailResult;

  const { data: albums, error: albumsError } = await client.GET("/api/albums", {
    params: {
      query: {
        page: filters.page,
        pageSize: PAGE_SIZE,
        startYear: decade.startYear ?? undefined,
        endYear: decade.endYear ?? undefined,
        sort: filters.sort,
      },
    },
  });

  if (albumsError) {
    throw new Error("Failed to load albums.");
  }

  const totalPages =
    albums.totalPages ?? Math.ceil(albums.totalCount / PAGE_SIZE);

  return (
    <>
      <Breadcrumb className="px-16 pt-8">
        <BreadcrumbList className="gap-2 text-xs font-semibold text-muted-foreground sm:gap-2">
          <BreadcrumbItem>
            <BreadcrumbLink
              render={<Link href="/decades">Decades</Link>}
              className="hover:text-ink"
            />
          </BreadcrumbItem>
          <BreadcrumbSeparator className="text-line">/</BreadcrumbSeparator>
          <BreadcrumbItem>
            <BreadcrumbPage className="text-ink">{decade.label}</BreadcrumbPage>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>

      <div className="mx-auto flex w-full max-w-275 flex-col gap-4 border-b border-line px-16 py-9">
        <span className="text-[0.6875rem] font-extrabold tracking-[0.08em] text-muted-foreground uppercase">
          Released in the
        </span>
        <h1 className="font-serif text-[2.625rem] leading-tight font-medium italic">
          {decade.label}
        </h1>
        {decade.description && (
          <p className="max-w-140 text-sm leading-[1.7] text-muted-foreground">
            {decade.description}
          </p>
        )}
        <span className="text-sm font-semibold text-muted-foreground">
          {decade.albumCount} {decade.albumCount === 1 ? "record" : "records"}
        </span>
      </div>

      <div className="mx-auto w-full max-w-275 border-b border-line px-16 py-6">
        <CategoryPillNav
          ariaLabel="Jump to another decade"
          activeKey={slug}
          items={decades.map((item) => ({
            key: item.slug,
            label: item.label,
            href: `/decades/${item.slug}`,
          }))}
        />
      </div>

      <div className="mx-auto flex w-full max-w-275 justify-end px-16 pt-6">
        <CatalogSortSelect basePath={`/decades/${slug}`} filters={filters} />
      </div>

      <div className="mx-auto w-full max-w-275 px-16 pt-6 pb-10">
        {albums.items.length === 0 ? (
          <p className="text-sm text-muted-foreground">
            No records from this decade yet.
          </p>
        ) : (
          <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-4">
            {albums.items.map((album) => (
              <ProductCard key={album.sqid} album={album} showReleaseYear />
            ))}
          </div>
        )}
      </div>

      <div className="mx-auto w-full max-w-275 px-16 pb-25">
        <CatalogPagination
          basePath={`/decades/${slug}`}
          filters={filters}
          totalPages={totalPages}
        />
      </div>
    </>
  );
}

function DecadeLandingSkeleton() {
  return (
    <div className="flex flex-1 items-center justify-center px-16 py-20 text-sm text-muted-foreground">
      Loading…
    </div>
  );
}

export default function DecadeLandingPage(props: PageProps<"/decades/[slug]">) {
  return (
    <Suspense fallback={<DecadeLandingSkeleton />}>
      <DecadeLandingContent
        params={props.params}
        searchParams={props.searchParams}
      />
    </Suspense>
  );
}
