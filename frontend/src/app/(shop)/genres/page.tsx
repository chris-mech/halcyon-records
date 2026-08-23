import { Suspense } from "react";
import type { Metadata } from "next";
import { connection } from "next/server";
import { cacheLife } from "next/cache";

import { client } from "@/lib/api/client";
import { CategoryTile } from "@/components/category-tile";
import type { components } from "@/lib/api/schema";

export const metadata: Metadata = {
  title: "Genres",
  description: "Browse the catalogue by genre.",
};

type GenreListItem = components["schemas"]["GenreListItemResponse"];

type GenresDataResult = { ok: true; genres: GenreListItem[] } | { ok: false };

async function getGenresData(): Promise<GenresDataResult> {
  "use cache";
  cacheLife({ stale: 60, revalidate: 60, expire: 300 });

  const { data, error } = await client.GET("/api/genres");

  if (error) {
    return { ok: false };
  }

  return { ok: true, genres: data };
}

export async function GenresContent() {
  await connection();

  const result = await getGenresData();

  if (!result.ok) {
    throw new Error("Failed to load genres.");
  }

  return (
    <>
      <div className="mx-auto flex w-full max-w-275 flex-col gap-3 px-16 pt-14 pb-8">
        <span className="text-[0.6875rem] font-extrabold tracking-[0.08em] text-muted-foreground uppercase">
          Index
        </span>
        <h1 className="font-serif text-4xl font-medium italic">Genres</h1>
        <p className="max-w-130 text-sm text-muted-foreground">
          Everything in the catalogue, sorted properly.
        </p>
      </div>

      <div className="mx-auto grid w-full max-w-275 grid-cols-2 gap-6 px-16 pb-25 sm:grid-cols-3 lg:grid-cols-4">
        {result.genres.map((genre) => (
          <CategoryTile
            key={genre.slug}
            href={`/genres/${genre.slug}`}
            name={genre.name}
            imageUrl={genre.imageUrl}
            albumCount={genre.albumCount}
          />
        ))}
      </div>
    </>
  );
}

function GenresSkeleton() {
  return (
    <div className="flex flex-1 items-center justify-center px-16 py-20 text-sm text-muted-foreground">
      Loading…
    </div>
  );
}

export default function GenresPage() {
  return (
    <Suspense fallback={<GenresSkeleton />}>
      <GenresContent />
    </Suspense>
  );
}
