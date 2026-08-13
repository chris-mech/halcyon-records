import { Fragment, Suspense } from "react";
import Image from "next/image";
import Link from "next/link";
import { cacheLife } from "next/cache";
import { DiscAlbum } from "lucide-react";

import { client } from "@/lib/api/client";
import { formatPrice } from "@/lib/format";
import { GenrePillList } from "@/components/genre-pill-list";
import { ShadowStackHeading } from "@/components/shadow-stack-heading";
import { Button } from "@/components/ui/button";
import type { components } from "@/lib/api/schema";

import { AlbumGridSection } from "./album-grid-section";

type CoverStory = components["schemas"]["CoverStoryResponse"];
type AlbumSummary = components["schemas"]["AlbumSummaryResponse"];

type HomepageDataResult =
  | {
      ok: true;
      coverStory: CoverStory;
      newArrivals: AlbumSummary[];
      onSaleAlbums: AlbumSummary[];
    }
  | { ok: false };

async function getHomepageData(): Promise<HomepageDataResult> {
  "use cache";
  cacheLife({ stale: 60, revalidate: 60, expire: 300 });

  const [
    { data: coverStory, error: coverStoryError },
    { data: newArrivals, error: newArrivalsError },
    { data: onSale, error: onSaleError },
  ] = await Promise.all([
    client.GET("/api/albums/cover-story"),
    client.GET("/api/albums", {
      params: {
        query: {
          page: 1,
          pageSize: 4,
          isNew: true,
          isOnSale: false,
          isStaffPick: false,
          sort: "NewestFirst",
        },
      },
    }),
    client.GET("/api/albums", {
      params: {
        query: {
          page: 1,
          pageSize: 4,
          isNew: false,
          isOnSale: true,
          isStaffPick: false,
          sort: "NewestFirst",
        },
      },
    }),
  ]);

  if (coverStoryError) {
    return { ok: false };
  }

  return {
    ok: true,
    coverStory,
    newArrivals: newArrivalsError ? [] : newArrivals.items,
    onSaleAlbums: onSaleError ? [] : onSale.items,
  };
}

export async function HomeContent() {
  const result = await getHomepageData();

  if (!result.ok) {
    throw new Error("Failed to load the homepage.");
  }

  const { coverStory, newArrivals, onSaleAlbums } = result;
  const albumHref = `/albums/${coverStory.sqid}/${coverStory.titleSlug}`;

  const quicklinks = [
    {
      label: "Staff picks",
      sub: "What we'd take home",
      href: "/shop?isStaffPick=true",
    },
    {
      label: "Browse all genres",
      sub: "Everything, sorted properly",
      href: "/genres",
    },
    {
      label: "Browse by decade",
      sub: "A trip through the decades",
      href: "/decades",
    },
    {
      label: "Browse by artist",
      sub: "Every artist on the shelf",
      href: "/artists",
    },
  ];

  return (
    <>
      <div className="mx-auto flex w-full max-w-275 items-baseline justify-between px-16 pt-14">
        <div>
          <span className="text-[0.9375rem] font-extrabold tracking-[0.08em] text-ink uppercase">
            This week&rsquo;s cover story
          </span>
          <span className="mt-1.5 block text-[0.6875rem] text-muted-foreground">
            Refreshed every Monday, pulled from what&rsquo;s actually on the
            shelf
          </span>
        </div>
        <span className="text-xs font-semibold text-muted-foreground">
          No. {String(coverStory.issueNumber).padStart(3, "0")}
        </span>
      </div>

      <section className="mx-auto grid w-full max-w-275 grid-cols-1 items-center gap-16 px-16 py-14 lg:grid-cols-[0.85fr_1.15fr]">
        <Link
          href={albumHref}
          className="relative block aspect-square shadow-lg"
        >
          {coverStory.imageUrl ? (
            <Image
              src={coverStory.imageUrl}
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
        </Link>

        <div>
          <GenrePillList genres={coverStory.genres} className="mb-5.5" />

          <Link href={albumHref}>
            <ShadowStackHeading as="h1" size="hero" className="mb-6">
              {coverStory.title}
            </ShadowStackHeading>
          </Link>

          <div className="mb-7 flex flex-wrap gap-x-1 text-sm font-bold tracking-wide text-muted-foreground uppercase">
            {coverStory.artists.map((artist, index) => (
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

          {coverStory.description && (
            <p className="mb-8 max-w-115 font-serif text-xl leading-[1.6] italic">
              &ldquo;{coverStory.description}&rdquo;
            </p>
          )}

          <div className="flex items-center gap-6">
            <Button type="button" size="lg" disabled={!coverStory.isInStock}>
              Add to bag
            </Button>
            <span className="text-sm font-semibold text-muted-foreground">
              {formatPrice(coverStory.priceInPence)}
            </span>
          </div>
        </div>
      </section>

      <hr className="mx-auto w-full max-w-275 border-line" />

      <AlbumGridSection
        heading="New arrivals"
        subtext="Just landed on the shelf"
        viewAllHref="/shop?isNew=true"
        albums={newArrivals}
      />

      <AlbumGridSection
        heading="On sale"
        subtext="Marked down, while it lasts"
        viewAllHref="/shop?isOnSale=true"
        albums={onSaleAlbums}
      />

      <section className="mx-auto w-full max-w-275 px-16 pb-24">
        <div className="flex border border-line">
          {quicklinks.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className="flex-1 border-r border-line px-5 py-6.5 text-center text-sm font-bold tracking-wide text-ink uppercase last:border-r-0"
            >
              {link.label}
              <span className="mt-1.5 block text-xs font-normal tracking-normal text-muted-foreground normal-case">
                {link.sub}
              </span>
            </Link>
          ))}
        </div>
      </section>
    </>
  );
}

function HomeSkeleton() {
  return (
    <div className="flex flex-1 items-center justify-center px-16 py-20 text-sm text-muted-foreground">
      Loading…
    </div>
  );
}

export default function HomePage() {
  return (
    <Suspense fallback={<HomeSkeleton />}>
      <HomeContent />
    </Suspense>
  );
}
