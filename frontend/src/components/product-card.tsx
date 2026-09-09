import Link from "next/link";

import { formatPrice } from "@/lib/format";
import type { components } from "@/lib/api/schema";
import { Card } from "@/components/ui/card";
import { AddToCartButton } from "@/components/add-to-cart-button";
import { AlbumTagStack } from "@/components/album-tag-stack";
import { InlineLinkList } from "@/components/inline-link-list";
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
        <div className="line-clamp-2 min-h-[2lh] text-[0.6875rem] font-bold tracking-wide text-muted-foreground uppercase">
          <InlineLinkList
            items={album.artists.map((artist) => ({
              id: artist.sqid,
              href: `/artists/${artist.sqid}/${artist.nameSlug}`,
              label: artist.name,
            }))}
            linkClassName="outline-none hover:underline focus-visible:ring-3 focus-visible:ring-ring"
          />
          {showGenre && album.genres.length > 0 && (
            <>
              <span className="sr-only">. </span>
              <span aria-hidden className="mx-2">
                |
              </span>
              <InlineLinkList
                items={album.genres.map((genre) => ({
                  id: genre.slug,
                  href: `/genres/${genre.slug}`,
                  label: genre.name,
                }))}
                linkClassName="outline-none hover:underline focus-visible:ring-3 focus-visible:ring-ring"
              />
            </>
          )}
          {showReleaseYear && releaseYear != null && (
            <>
              <span className="sr-only">. </span>
              <span aria-hidden className="mx-2">
                |
              </span>
              <span className="text-[0.6875rem] font-semibold text-muted-foreground">
                {releaseYear}
              </span>
            </>
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
          <AddToCartButton
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
