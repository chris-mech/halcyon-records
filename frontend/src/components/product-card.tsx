import { Fragment } from "react";
import Image from "next/image";
import Link from "next/link";
import { DiscAlbum } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Card } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import type { components } from "@/lib/api/schema";

type AlbumSummary = components["schemas"]["AlbumSummaryResponse"];

interface ProductCardProps {
  album: AlbumSummary;
  showGenre?: boolean;
}

const currencyFormatter = new Intl.NumberFormat("en-GB", {
  style: "currency",
  currency: "GBP",
});

const tagClassName =
  "h-auto px-3 py-1 text-[0.625rem] font-bold tracking-wide uppercase shadow-sm";

function formatPrice(pence: number) {
  return currencyFormatter.format(pence / 100);
}

function ProductCard({ album, showGenre = true }: ProductCardProps) {
  const albumHref = `/albums/${album.sqid}/${album.titleSlug}`;
  const hasTags = album.isNew || album.isOnSale || album.isStaffPick;

  return (
    <Card className="relative gap-0 overflow-visible py-0">
      {hasTags && (
        <div className="absolute -top-1.5 right-5 z-10 flex flex-row gap-1">
          {album.isNew && <Badge className={tagClassName}>New</Badge>}
          {album.isOnSale && (
            <Badge className={cn(tagClassName, "bg-gold text-ink")}>
              On sale
            </Badge>
          )}
          {album.isStaffPick && (
            <Badge variant="secondary" className={tagClassName}>
              Staff pick
            </Badge>
          )}
        </div>
      )}

      <Link
        href={albumHref}
        aria-hidden
        tabIndex={-1}
        className="relative block aspect-square border-b border-line bg-slate-muted/40"
      >
        {album.imageUrl ? (
          <Image
            src={album.imageUrl}
            alt=""
            fill
            sizes="(min-width: 1024px) 25vw, (min-width: 640px) 33vw, 50vw"
            className="object-cover"
          />
        ) : (
          <div className="flex size-full items-center justify-center">
            <DiscAlbum aria-hidden className="size-10 text-slate-muted" />
          </div>
        )}
      </Link>

      <div className="flex flex-col gap-2.5 p-4">
        <div className="flex flex-wrap items-center gap-x-1 text-[0.6875rem] font-bold tracking-wide text-muted-foreground uppercase">
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
          {showGenre && album.genres.length > 0 && (
            <>
              <span aria-hidden>·</span>
              {album.genres.map((genre, index) => (
                <Fragment key={genre.slug}>
                  {index > 0 && ", "}
                  <Link
                    href={`/genres/${genre.slug}`}
                    className="hover:underline"
                  >
                    {genre.name}
                  </Link>
                </Fragment>
              ))}
            </>
          )}
        </div>

        <Link
          href={albumHref}
          className="font-serif text-[1.0625rem] font-medium italic hover:underline"
        >
          {album.title}
        </Link>

        <div className="flex items-center justify-between border-t border-line pt-2.5">
          <p className="flex items-baseline">
            {album.originalPriceInPence != null && (
              <span className="mr-1.5 text-sm font-medium text-muted-foreground line-through">
                {formatPrice(album.originalPriceInPence)}
              </span>
            )}
            <span className="text-sm font-bold text-ink">
              {formatPrice(album.priceInPence)}
            </span>
          </p>
          <button
            type="button"
            disabled={!album.isInStock}
            className="text-[0.6875rem] font-bold tracking-wide text-slate uppercase hover:underline disabled:cursor-not-allowed disabled:text-muted-foreground disabled:no-underline disabled:opacity-60"
          >
            Add to bag
          </button>
        </div>
      </div>
    </Card>
  );
}

export { ProductCard };
