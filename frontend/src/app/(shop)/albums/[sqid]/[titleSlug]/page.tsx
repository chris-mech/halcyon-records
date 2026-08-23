import { Fragment, Suspense } from "react";
import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound, permanentRedirect } from "next/navigation";
import { cacheLife } from "next/cache";
import { DiscAlbum } from "lucide-react";

import { client } from "@/lib/api/client";
import { formatPrice } from "@/lib/format";
import { AlbumTagStack } from "@/components/album-tag-stack";
import { GenrePillList } from "@/components/genre-pill-list";
import { ProductCard } from "@/components/product-card";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";

import { PurchaseRow } from "./purchase-row";

import type { components } from "@/lib/api/schema";

export async function generateMetadata({
  params,
}: Pick<PageProps<"/albums/[sqid]/[titleSlug]">, "params">): Promise<Metadata> {
  const { sqid } = await params;
  const result = await getAlbumData(sqid);

  if (!result.found) {
    notFound();
  }

  const { album } = result;
  const artistNames = album.artists.map((artist) => artist.name).join(", ");

  return {
    title: artistNames ? `${album.title} by ${artistNames}` : album.title,
    description: album.description ?? undefined,
    openGraph: album.imageUrl ? { images: [album.imageUrl] } : undefined,
  };
}

type AlbumDetail = components["schemas"]["AlbumDetailResponse"];
type RelatedAlbum = components["schemas"]["RelatedAlbumResponse"];

type AlbumDetailResult =
  | { found: true; album: AlbumDetail; relatedAlbums: RelatedAlbum[] }
  | { found: false };

const LOW_STOCK_THRESHOLD = 5;

function stockNote(unitsInStock: number): string | null {
  if (unitsInStock === 0) return "Out of stock";
  if (unitsInStock <= LOW_STOCK_THRESHOLD) {
    return `Only ${unitsInStock} left in stock`;
  }
  return null;
}

async function getAlbumData(sqid: string): Promise<AlbumDetailResult> {
  "use cache";
  cacheLife({ stale: 60, revalidate: 60, expire: 300 });

  const [
    { data: album, error: albumError },
    { data: related, error: relatedError },
  ] = await Promise.all([
    client.GET("/api/albums/{sqid}", { params: { path: { sqid } } }),
    client.GET("/api/albums/{sqid}/related", { params: { path: { sqid } } }),
  ]);

  if (albumError) {
    return { found: false };
  }

  return { found: true, album, relatedAlbums: relatedError ? [] : related };
}

export async function AlbumDetailContent({
  params,
}: Pick<PageProps<"/albums/[sqid]/[titleSlug]">, "params">) {
  const { sqid, titleSlug } = await params;
  const result = await getAlbumData(sqid);

  if (!result.found) {
    notFound();
  }

  const { album, relatedAlbums } = result;
  const artistNames = album.artists.map((artist) => artist.name).join(", ");

  if (album.titleSlug !== titleSlug) {
    permanentRedirect(`/albums/${sqid}/${album.titleSlug}`);
  }

  const note = stockNote(album.unitsInStock);
  const hasMeta = album.genres.length > 0 || album.releaseDate || album.label;

  return (
    <>
      <Breadcrumb className="px-16 pt-8">
        <BreadcrumbList className="gap-2 text-xs font-semibold text-muted-foreground sm:gap-2">
          <BreadcrumbItem>
            <BreadcrumbLink
              render={<Link href="/shop">Shop</Link>}
              className="hover:text-ink"
            />
          </BreadcrumbItem>
          <BreadcrumbSeparator className="text-line">/</BreadcrumbSeparator>
          <BreadcrumbItem>
            <BreadcrumbPage className="text-ink">{album.title}</BreadcrumbPage>
          </BreadcrumbItem>
        </BreadcrumbList>
      </Breadcrumb>

      <section className="mx-auto grid w-full max-w-275 grid-cols-1 gap-16 px-16 py-10 lg:grid-cols-[0.85fr_1.15fr]">
        <div className="relative">
          <AlbumTagStack
            isNew={album.isNew}
            isOnSale={album.isOnSale}
            isStaffPick={album.isStaffPick}
            className="absolute -top-3 left-6 z-10"
          />
          <div className="relative aspect-square overflow-hidden shadow-lg">
            {album.imageUrl ? (
              <Image
                src={album.imageUrl}
                alt={
                  artistNames
                    ? `${album.title} by ${artistNames} — album cover`
                    : `${album.title} — album cover`
                }
                fill
                sizes="(min-width: 1024px) 40vw, 90vw"
                className="object-cover"
              />
            ) : (
              <div className="flex size-full items-center justify-center bg-slate-muted/40">
                <DiscAlbum aria-hidden className="size-16 text-slate" />
              </div>
            )}
          </div>
        </div>

        <div className="flex flex-col">
          <GenrePillList genres={album.genres} className="mb-5" />

          <div className="mb-2 flex flex-wrap gap-x-1 text-sm font-bold tracking-wide text-muted-foreground uppercase">
            {album.artists.map((artist, index) => (
              <Fragment key={artist.sqid}>
                {index > 0 && ", "}
                <Link
                  href={`/artists/${artist.sqid}/${artist.nameSlug}`}
                  className="hover:underline"
                >
                  {artist.name}
                </Link>
              </Fragment>
            ))}
          </div>

          <h1 className="mb-5 font-serif text-[2.625rem] leading-tight font-medium italic">
            {album.title}
          </h1>

          {album.description && (
            <p className="mb-7 max-w-115 text-[0.9375rem] leading-[1.75] text-muted-foreground">
              {album.description}
            </p>
          )}

          <div className="mb-2 flex items-baseline gap-3.5">
            {album.originalPriceInPence != null && (
              <span className="text-base font-medium text-muted-foreground line-through">
                {formatPrice(album.originalPriceInPence)}
              </span>
            )}
            <span className="text-[1.75rem] font-bold text-ink">
              {formatPrice(album.priceInPence)}
            </span>
          </div>

          {note && (
            <p className="mb-7 text-xs font-bold tracking-wide text-rust uppercase">
              {note}
            </p>
          )}

          <PurchaseRow album={album} />

          {hasMeta && (
            <dl className="border-t border-line pt-5">
              {album.genres.length > 0 && (
                <div className="flex justify-between border-b border-line py-2.5 text-sm">
                  <dt className="font-semibold text-muted-foreground">Genre</dt>
                  <dd className="font-semibold">
                    {album.genres.map((genre) => genre.name).join(", ")}
                  </dd>
                </div>
              )}
              {album.releaseDate && (
                <div className="flex justify-between border-b border-line py-2.5 text-sm">
                  <dt className="font-semibold text-muted-foreground">
                    Released
                  </dt>
                  <dd className="font-semibold">
                    {album.releaseDate.slice(0, 4)}
                  </dd>
                </div>
              )}
              {album.label && (
                <div className="flex justify-between border-b border-line py-2.5 text-sm">
                  <dt className="font-semibold text-muted-foreground">Label</dt>
                  <dd className="font-semibold">{album.label}</dd>
                </div>
              )}
            </dl>
          )}
        </div>
      </section>

      {relatedAlbums.length > 0 && (
        <section className="mx-auto w-full max-w-275 px-16 pb-25">
          <h2 className="mb-11 text-center font-heading text-3xl font-black uppercase">
            More in this mood
          </h2>
          <div className="grid grid-cols-2 gap-7 sm:grid-cols-3 lg:grid-cols-4">
            {relatedAlbums.map((related) => (
              <ProductCard key={related.sqid} album={related} />
            ))}
          </div>
        </section>
      )}
    </>
  );
}

function AlbumDetailSkeleton() {
  return (
    <div className="flex flex-1 items-center justify-center px-16 py-20 text-sm text-muted-foreground">
      Loading…
    </div>
  );
}

export default function AlbumDetailPage(
  props: PageProps<"/albums/[sqid]/[titleSlug]">,
) {
  return (
    <Suspense fallback={<AlbumDetailSkeleton />}>
      <AlbumDetailContent params={props.params} />
    </Suspense>
  );
}
