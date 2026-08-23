import { Suspense } from "react";
import type { Metadata } from "next";
import { connection } from "next/server";
import { cacheLife } from "next/cache";

import { client } from "@/lib/api/client";
import { CategoryTile } from "@/components/category-tile";
import type { components } from "@/lib/api/schema";

export const metadata: Metadata = {
  title: "Decades",
  description: "Browse the catalogue by decade.",
};

type DecadeListItem = components["schemas"]["DecadeListItemResponse"];

type DecadesDataResult =
  { ok: true; decades: DecadeListItem[] } | { ok: false };

async function getDecadesData(): Promise<DecadesDataResult> {
  "use cache";
  cacheLife({ stale: 60, revalidate: 60, expire: 300 });

  const { data, error } = await client.GET("/api/decades");

  if (error) {
    return { ok: false };
  }

  return { ok: true, decades: data };
}

export async function DecadesContent() {
  await connection();

  const result = await getDecadesData();

  if (!result.ok) {
    throw new Error("Failed to load decades.");
  }

  return (
    <>
      <div className="mx-auto flex w-full max-w-275 flex-col gap-3 px-16 pt-14 pb-8">
        <span className="text-[0.6875rem] font-extrabold tracking-[0.08em] text-muted-foreground uppercase">
          Index
        </span>
        <h1 className="font-serif text-4xl font-medium italic">
          Browse by decade
        </h1>
        <p className="max-w-130 text-sm text-muted-foreground">
          A trip through the decades — everything sorted by when it actually
          came out.
        </p>
      </div>

      <div className="mx-auto grid w-full max-w-275 grid-cols-2 gap-6 px-16 pb-25 sm:grid-cols-3 lg:grid-cols-4">
        {result.decades.map((decade) => (
          <CategoryTile
            key={decade.slug}
            href={`/decades/${decade.slug}`}
            name={decade.label}
            imageUrl={decade.imageUrl}
            albumCount={decade.albumCount}
          />
        ))}
      </div>
    </>
  );
}

function DecadesSkeleton() {
  return (
    <div className="flex flex-1 items-center justify-center px-16 py-20 text-sm text-muted-foreground">
      Loading…
    </div>
  );
}

export default function DecadesPage() {
  return (
    <Suspense fallback={<DecadesSkeleton />}>
      <DecadesContent />
    </Suspense>
  );
}
