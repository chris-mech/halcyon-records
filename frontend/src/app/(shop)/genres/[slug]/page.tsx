import { Suspense } from "react";
import { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { cacheLife } from "next/cache";

import { client } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";
import { PAGE_SIZE, parseCatalogFilters } from "@/lib/catalog-search-params";
import { LoadingState } from "@/components/loading-state";
import { SkeletonCardGrid } from "@/components/skeleton-primitives";
import { Skeleton } from "@/components/ui/skeleton";
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
}: Pick<PageProps<"/genres/[slug]">, "params">): Promise<Metadata> {
  const { slug } = await params;
  const result = await getGenreDetailData(slug);

  if (!result.found) {
    notFound();
  }

  const { genre } = result;

  return {
    title: genre.name,
    description: genre.description ?? undefined,
    openGraph: genre.imageUrl ? { images: [genre.imageUrl] } : undefined,
  };
}

type GenreDetail = components["schemas"]["GenreDetailResponse"];
type GenreListItem = components["schemas"]["GenreListItemResponse"];

type GenreDetailResult = { found: true; genre: GenreDetail } | { found: false };

async function getGenreDetailData(slug: string): Promise<GenreDetailResult> {
  "use cache";
  cacheLife({ stale: 60, revalidate: 60, expire: 300 });

  const { data: genre, error } = await client.GET("/api/genres/{slug}", {
    params: { path: { slug } },
  });

  if (error) {
    return { found: false };
  }

  return { found: true, genre };
}

async function getGenreListData(): Promise<GenreListItem[]> {
  "use cache";
  cacheLife({ stale: 60, revalidate: 60, expire: 300 });

  const { data, error } = await client.GET("/api/genres");

  return error ? [] : data;
}

export async function GenreLandingContent({
  params,
  searchParams,
}: Pick<PageProps<"/genres/[slug]">, "params" | "searchParams">) {
  const { slug } = await params;
  const filters = parseCatalogFilters(await searchParams);

  const [detailResult, genres] = await Promise.all([
    getGenreDetailData(slug),
    getGenreListData(),
  ]);

  if (!detailResult.found) {
    notFound();
  }

  const { genre } = detailResult;

  const { data: albums, error: albumsError } = await client.GET("/api/albums", {
    params: {
      query: {
        page: filters.page,
        pageSize: PAGE_SIZE,
        genres: [slug],
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
              render={<Link href="/genres">Genres</Link>}
              className="hover:text-ink"
            />
          </BreadcrumbItem>
          <BreadcrumbSeparator className="text-line">/</BreadcrumbSeparator>
          <BreadcrumbItem>
            <BreadcrumbPage className="text-ink">{genre.name}</BreadcrumbPage>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>

      <div className="mx-auto flex w-full max-w-275 flex-col gap-4 border-b border-line px-16 py-9">
        <span className="text-[0.6875rem] font-extrabold tracking-[0.08em] text-muted-foreground uppercase">
          Genre
        </span>
        <h1 className="font-serif text-[2.625rem] leading-tight font-medium italic">
          {genre.name}
        </h1>
        {genre.description && (
          <p className="max-w-140 text-sm leading-[1.7] text-muted-foreground">
            {genre.description}
          </p>
        )}
        <span className="text-sm font-semibold text-muted-foreground">
          {genre.albumCount} {genre.albumCount === 1 ? "record" : "records"}
        </span>
      </div>

      <div className="mx-auto w-full max-w-275 border-b border-line px-16 py-6">
        <CategoryPillNav
          ariaLabel="Jump to another genre"
          activeKey={slug}
          items={genres.map((item) => ({
            key: item.slug,
            label: item.name,
            href: `/genres/${item.slug}`,
          }))}
        />
      </div>

      <div className="mx-auto flex w-full max-w-275 justify-end px-16 pt-6">
        <CatalogSortSelect basePath={`/genres/${slug}`} filters={filters} />
      </div>

      <div className="mx-auto w-full max-w-275 px-16 pt-6 pb-10">
        {albums.items.length === 0 ? (
          <p className="text-sm text-muted-foreground">
            No records in this genre yet.
          </p>
        ) : (
          <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-4">
            {albums.items.map((album) => (
              <ProductCard key={album.sqid} album={album} showGenre={false} />
            ))}
          </div>
        )}
      </div>

      <div className="mx-auto w-full max-w-275 px-16 pb-25">
        <CatalogPagination
          basePath={`/genres/${slug}`}
          filters={filters}
          totalPages={totalPages}
        />
      </div>
    </>
  );
}

function GenreLandingSkeleton() {
  return (
    <LoadingState>
      <div className="mx-auto flex w-full max-w-275 flex-col gap-4 border-b border-line px-16 py-9">
        <Skeleton className="h-3 w-14" />
        <Skeleton className="h-11 w-72" />
        <Skeleton className="h-4 w-full max-w-140" />
      </div>
      <div className="mx-auto w-full max-w-275 px-16 pt-6 pb-10">
        <SkeletonCardGrid />
      </div>
    </LoadingState>
  );
}

export default function GenreLandingPage(props: PageProps<"/genres/[slug]">) {
  return (
    <Suspense fallback={<GenreLandingSkeleton />}>
      <GenreLandingContent
        params={props.params}
        searchParams={props.searchParams}
      />
    </Suspense>
  );
}
