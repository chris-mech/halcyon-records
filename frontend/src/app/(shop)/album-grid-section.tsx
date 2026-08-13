import Link from "next/link";

import { ProductCard } from "@/components/product-card";
import { ShadowStackHeading } from "@/components/shadow-stack-heading";
import type { components } from "@/lib/api/schema";

type AlbumSummary = components["schemas"]["AlbumSummaryResponse"];

interface AlbumGridSectionProps {
  heading: string;
  subtext: string;
  viewAllHref: string;
  albums: AlbumSummary[];
}

function AlbumGridSection({
  heading,
  subtext,
  viewAllHref,
  albums,
}: AlbumGridSectionProps) {
  if (albums.length === 0) {
    return null;
  }

  return (
    <section className="mx-auto w-full max-w-275 px-16 py-12">
      <div className="mb-11 flex flex-col items-center gap-3.5 text-center">
        <ShadowStackHeading as="h2" size="section">
          {heading}
        </ShadowStackHeading>
        <div className="flex items-center gap-4">
          <p className="text-sm font-semibold text-muted-foreground">
            {subtext}
          </p>
          <Link
            href={viewAllHref}
            className="border-b-2 border-ink pb-px text-sm font-bold text-ink"
          >
            View all →
          </Link>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-4">
        {albums.map((album) => (
          <ProductCard key={album.sqid} album={album} />
        ))}
      </div>
    </section>
  );
}

export { AlbumGridSection };
