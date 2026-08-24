import { Fragment } from "react";
import Link from "next/link";

import { formatPrice } from "@/lib/format";
import type { components } from "@/lib/api/schema";
import { Card } from "@/components/ui/card";
import { AddToBagButton } from "@/components/add-to-bag-button";
import { AlbumTagStack } from "@/components/album-tag-stack";
import { MediaThumbnail } from "./media-thumbnail";

type AlbumSummary = components["schemas"]["AlbumSummaryResponse"];

interface ProductCardProps {
  album: AlbumSummary;
  showGenre?: boolean;
  showReleaseYear?: boolean;
}

function ProductCard({
  album,
  showGenre = true,
  showReleaseYear = false,
}: ProductCardProps) {
  const albumHref = `/albums/${album.sqid}/${album.titleSlug}`;
  const releaseYear = album.releaseDate
    ? new Date(album.releaseDate).getFullYear()
    : null;

  return (
    <Card className="relative gap-0 overflow-visible py-0">
      <AlbumTagStack
        isNew={album.isNew}
        isOnSale={album.isOnSale}
        isStaffPick={album.isStaffPick}
        className="absolute -top-1.5 left-5 z-10"
      />

      <MediaThumbnail
        imageUrl={album.imageUrl}
        href={albumHref}
        sizes="(min-width: 1024px) 25vw, (min-width: 640px) 33vw, 50vw"
        className="block aspect-square border-b border-line"
      />

      <div className="flex flex-1 flex-col gap-2.5 p-4">
        <div className="flex flex-wrap items-center gap-x-1 text-[0.6875rem] font-bold tracking-wide text-muted-foreground uppercase">
          {album.artists.map((artist, index) => (
            <Fragment key={artist.sqid}>
              {index > 0 && ", "}
              <Link
                href={`/artists/${artist.sqid}/${artist.nameSlug}`}
                className="outline-none hover:underline focus-visible:ring-3 focus-visible:ring-ring"
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
                    className="outline-none hover:underline focus-visible:ring-3 focus-visible:ring-ring"
                  >
                    {genre.name}
                  </Link>
                </Fragment>
              ))}
            </>
          )}
          {showReleaseYear && releaseYear != null && (
            <span className="text-[0.6875rem] font-semibold text-muted-foreground">
              {releaseYear}
            </span>
          )}
        </div>

        <Link
          href={albumHref}
          className="font-serif text-[1.0625rem] font-medium italic outline-none hover:underline focus-visible:ring-3 focus-visible:ring-ring"
        >
          {album.title}
        </Link>

        <div className="mt-auto flex items-center justify-between border-t border-line pt-2.5">
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
          <AddToBagButton
            album={album}
            variant="link"
            className="h-auto p-0 text-[0.6875rem] font-bold tracking-wide text-slate uppercase"
          />
        </div>
      </div>
    </Card>
  );
}

export { ProductCard };
