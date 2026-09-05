import { Suspense } from "react";
import { Metadata } from "next";
import Link from "next/link";
import { notFound, permanentRedirect } from "next/navigation";
import { cacheLife } from "next/cache";

import type { components } from "@/lib/api/schema";
import { client } from "@/lib/api/client";
import { SITE_OPEN_GRAPH_DEFAULTS } from "@/lib/site-config";
import { assertOk } from "@/lib/api/assert-ok";
import { LoadingState } from "@/components/loading-state";
import {
  SkeletonCardGrid,
  SkeletonLines,
} from "@/components/skeleton-primitives";
import { Skeleton } from "@/components/ui/skeleton";
import { GenrePillList } from "@/components/genre-pill-list";
import { ProductCard } from "@/components/product-card";
import { MediaThumbnail } from "@/components/media-thumbnail";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";

import { ArtistSortSelect } from "./artist-sort-select";
import { parseArtistSort, type ArtistAlbumSort } from "./search-params";

export async function generateMetadata({
  params,
  searchParams,
}: Pick<
  PageProps<"/artists/[sqid]/[nameSlug]">,
  "params" | "searchParams"
>): Promise<Metadata> {
  const { sqid } = await params;
  const sort = parseArtistSort(await searchParams);
  const result = await getArtistDetailData(sqid, sort);

  if (!result.found) {
    notFound();
  }

  const { artist } = result;

  return {
    title: artist.name,
    description: artist.bio ?? undefined,
    ...(artist.imageUrl && {
      openGraph: { ...SITE_OPEN_GRAPH_DEFAULTS, images: [artist.imageUrl] },
    }),
  };
}

type ArtistDetail = components["schemas"]["ArtistDetailResponse"];

type ArtistDetailResult =
  { found: true; artist: ArtistDetail } | { found: false };

async function getArtistDetailData(
  sqid: string,
  sort: ArtistAlbumSort,
): Promise<ArtistDetailResult> {
  "use cache";
  cacheLife({ stale: 60, revalidate: 60, expire: 300 });

  const result = await client.GET("/api/artists/{sqid}", {
    params: { path: { sqid }, query: { sort } },
  });

  if (result.response.status === 404) {
    return { found: false };
  }

  const artist = assertOk(result, "Failed to load artist.");

  return { found: true, artist };
}

type ArtistType = components["schemas"]["ArtistType"];

function sinceYearLabel(sinceYear: number, type: ArtistType): string {
  if (type === "Person") return `Born ${sinceYear}`;
  if (type == null) return `Active since ${sinceYear}`;
  return `Formed ${sinceYear}`;
}

function originNote(
  origin: string | null,
  sinceYear: number | null,
  type: ArtistType,
): string | null {
  const yearPart = sinceYear == null ? null : sinceYearLabel(sinceYear, type);
  if (origin && yearPart) return `${origin} · ${yearPart}`;
  return origin ?? yearPart;
}

export async function ArtistDetailContent({
  params,
  searchParams,
}: Pick<PageProps<"/artists/[sqid]/[nameSlug]">, "params" | "searchParams">) {
  const { sqid, nameSlug } = await params;
  const sort = parseArtistSort(await searchParams);
  const result = await getArtistDetailData(sqid, sort);

  if (!result.found) {
    notFound();
  }

  const { artist } = result;

  if (artist.nameSlug !== nameSlug) {
    permanentRedirect(`/artists/${sqid}/${artist.nameSlug}`);
  }

  const note = originNote(artist.origin, artist.sinceYear, artist.type);

  return (
    <>
      <Breadcrumb className="px-16 pt-8">
        <BreadcrumbList className="gap-2 text-xs font-semibold text-muted-foreground sm:gap-2">
          <BreadcrumbItem>
            <BreadcrumbLink
              render={<Link href="/artists">Artists</Link>}
              className="hover:text-ink"
            />
          </BreadcrumbItem>
          <BreadcrumbSeparator className="text-line">/</BreadcrumbSeparator>
          <BreadcrumbItem>
            <BreadcrumbPage className="text-ink">{artist.name}</BreadcrumbPage>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>

      <section className="mx-auto grid w-full max-w-275 grid-cols-1 gap-10 border-b border-line px-16 py-9 sm:grid-cols-[200px_1fr] sm:items-center">
        <MediaThumbnail
          imageUrl={artist.imageUrl}
          alt={artist.name}
          sizes="200px"
          className="aspect-square w-50 overflow-hidden shadow-lg"
          iconClassName="size-12"
        />

        <div>
          <span className="mb-3.5 block text-[0.6875rem] font-extrabold tracking-[0.08em] text-muted-foreground uppercase">
            Artist
          </span>
          <h1 className="mb-3.5 font-serif text-[2.625rem] leading-tight font-medium italic">
            {artist.name}
          </h1>

          <div className="mb-4.5 flex flex-wrap items-center gap-3.5">
            <GenrePillList genres={artist.genres} />
            {note && (
              <span className="text-xs font-semibold text-muted-foreground">
                {note}
              </span>
            )}
          </div>

          {artist.bio && (
            <p className="max-w-130 text-sm leading-[1.7] text-muted-foreground">
              {artist.bio}
            </p>
          )}
        </div>
      </section>

      <div className="mx-auto flex w-full max-w-275 items-center justify-between px-16 pt-7">
        <span className="text-sm font-semibold text-muted-foreground">
          {artist.albumCount} {artist.albumCount === 1 ? "album" : "albums"} in
          our catalogue
        </span>
        <ArtistSortSelect sqid={sqid} nameSlug={artist.nameSlug} sort={sort} />
      </div>

      <div className="mx-auto w-full max-w-275 px-16 pt-6 pb-25">
        {artist.albums.length === 0 ? (
          <p className="text-sm text-muted-foreground">
            No albums in stock from this artist yet.
          </p>
        ) : (
          <div className="grid grid-cols-2 gap-7 sm:grid-cols-3 lg:grid-cols-4">
            {artist.albums.map((album) => (
              <ProductCard key={album.sqid} album={album} />
            ))}
          </div>
        )}
      </div>
    </>
  );
}

function ArtistDetailSkeleton() {
  return (
    <LoadingState>
      <section className="mx-auto grid w-full max-w-275 grid-cols-1 gap-10 border-b border-line px-16 py-9 sm:grid-cols-[200px_1fr] sm:items-center">
        <Skeleton className="aspect-square w-50 shadow-lg" />
        <div className="flex flex-col gap-3.5">
          <Skeleton className="h-3 w-16" />
          <Skeleton className="h-11 w-64" />
          <Skeleton className="h-5 w-40" />
          <SkeletonLines count={2} className="max-w-130" />
        </div>
      </section>
      <div className="mx-auto w-full max-w-275 px-16 pt-6 pb-25">
        <SkeletonCardGrid />
      </div>
    </LoadingState>
  );
}

export default function ArtistDetailPage(
  props: PageProps<"/artists/[sqid]/[nameSlug]">,
) {
  return (
    <Suspense fallback={<ArtistDetailSkeleton />}>
      <ArtistDetailContent
        params={props.params}
        searchParams={props.searchParams}
      />
    </Suspense>
  );
}
