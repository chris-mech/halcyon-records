import { Fragment } from "react";
import Image from "next/image";
import Link from "next/link";
import { notFound, permanentRedirect } from "next/navigation";
import { DiscAlbum } from "lucide-react";

import { client } from "@/lib/api/client";
import { formatPrice } from "@/lib/format";
import { cn } from "@/lib/utils";
import { AlbumTagStack } from "@/components/album-tag-stack";
import { ProductCard } from "@/components/product-card";
import { buttonVariants } from "@/components/ui/button";

import { PurchaseRow } from "./purchase-row";

const LOW_STOCK_THRESHOLD = 5;

function stockNote(unitsInStock: number): string | null {
  if (unitsInStock === 0) return "Out of stock";
  if (unitsInStock <= LOW_STOCK_THRESHOLD) {
    return `Only ${unitsInStock} left in stock`;
  }
  return null;
}

export default async function AlbumDetailPage(
  props: PageProps<"/albums/[sqid]/[titleSlug]">,
) {
  const { sqid, titleSlug } = await props.params;

  const [
    { data: album, error: albumError },
    { data: related, error: relatedError },
  ] = await Promise.all([
    client.GET("/api/albums/{sqid}", { params: { path: { sqid } } }),
    client.GET("/api/albums/{sqid}/related", { params: { path: { sqid } } }),
  ]);

  if (albumError) {
    notFound();
  }

  if (album.titleSlug !== titleSlug) {
    permanentRedirect(`/albums/${sqid}/${album.titleSlug}`);
  }

  const relatedAlbums = relatedError ? [] : related;
  const primaryGenre = album.genres[0] ?? null;
  const note = stockNote(album.unitsInStock);
  const hasMeta = album.genres.length > 0 || album.releaseDate || album.label;

  return (
    <>
      <nav
        aria-label="Breadcrumb"
        className="px-16 pt-8 text-xs font-semibold text-muted-foreground"
      >
        <ol className="flex items-center gap-2">
          <li>
            <Link href="/shop" className="hover:text-ink">
              Shop
            </Link>
          </li>
          {primaryGenre && (
            <>
              <li aria-hidden className="text-line">
                /
              </li>
              <li>
                <Link
                  href={`/genres/${primaryGenre.slug}`}
                  className="hover:text-ink"
                >
                  {primaryGenre.name}
                </Link>
              </li>
            </>
          )}
          <li aria-hidden className="text-line">
            /
          </li>
          <li className="text-ink" aria-current="page">
            {album.title}
          </li>
        </ol>
      </nav>

      <section className="mx-auto grid max-w-275 grid-cols-1 gap-12 px-16 py-10 lg:grid-cols-[0.85fr_1.15fr]">
        <div className="relative">
          <AlbumTagStack
            isNew={album.isNew}
            isOnSale={album.isOnSale}
            isStaffPick={album.isStaffPick}
            orientation="vertical"
            className="absolute -top-3 right-6 z-10"
          />
          <div className="relative aspect-square overflow-hidden shadow-lg">
            {album.imageUrl ? (
              <Image
                src={album.imageUrl}
                alt=""
                fill
                sizes="(min-width: 1024px) 40vw, 90vw"
                className="object-cover"
              />
            ) : (
              <div className="flex size-full items-center justify-center bg-slate-muted/40">
                <DiscAlbum aria-hidden className="size-16 text-slate-muted" />
              </div>
            )}
          </div>
        </div>

        <div className="flex flex-col">
          {primaryGenre && (
            <Link
              href={`/genres/${primaryGenre.slug}`}
              className={cn(
                buttonVariants({ variant: "outline" }),
                "mb-5 h-auto w-fit border-ink px-2.75 py-1 text-[0.625rem] font-bold tracking-wide text-ink uppercase",
              )}
            >
              {primaryGenre.name}
            </Link>
          )}

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
            <p className="mb-7 max-w-115 text-sm leading-relaxed text-muted-foreground">
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

          <PurchaseRow
            isInStock={album.isInStock}
            maxQuantity={album.unitsInStock}
          />

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
        <section className="mx-auto max-w-275 px-16 pb-25">
          <h2 className="mb-11 text-center font-heading text-3xl font-black uppercase">
            More in this mood
          </h2>
          <div className="grid grid-cols-2 gap-7 sm:grid-cols-3 lg:grid-cols-4">
            {relatedAlbums.map((related) => (
              <ProductCard
                key={related.sqid}
                album={related}
                showGenre={false}
              />
            ))}
          </div>
        </section>
      )}
    </>
  );
}
