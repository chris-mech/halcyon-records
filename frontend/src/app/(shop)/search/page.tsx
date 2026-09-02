import { Suspense } from "react";
import type { Metadata } from "next";
import Link from "next/link";
import { Search } from "lucide-react";

import { client } from "@/lib/api/client";
import { LoadingState } from "@/components/loading-state";
import { SkeletonCardGrid } from "@/components/skeleton-primitives";
import { Skeleton } from "@/components/ui/skeleton";
import { ProductCard } from "@/components/product-card";
import { EmptyState } from "@/components/empty-state";

export async function generateMetadata({
  searchParams,
}: Pick<PageProps<"/search">, "searchParams">): Promise<Metadata> {
  const query = firstValue((await searchParams).q)?.trim();

  return {
    title: query ? `Results for "${query}"` : "Search",
    description: query
      ? `Search results for "${query}" at Halcyon Records.`
      : "Search the Halcyon Records catalogue.",
  };
}

function firstValue(value: string | string[] | undefined): string | undefined {
  return Array.isArray(value) ? value[0] : value;
}

function SuggestedSearchTerms({ terms }: { terms: string[] }) {
  if (terms.length === 0) {
    return null;
  }

  return (
    <div className="border-t border-line pt-6 text-left">
      <div className="mb-3.5 text-[0.6875rem] font-bold tracking-wide text-muted-foreground uppercase">
        Try searching for
      </div>
      <ul className="flex flex-col">
        {terms.map((term) => (
          <li key={term}>
            <Link
              href={`/search?q=${encodeURIComponent(term)}`}
              className="block border-b border-line py-2 text-sm text-ink hover:underline"
            >
              {term}
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}

export async function SearchContent({
  searchParams,
}: Pick<PageProps<"/search">, "searchParams">) {
  const query = firstValue((await searchParams).q)?.trim();

  if (!query) {
    const { data: suggestions } = await client.GET("/api/search/suggestions");

    return (
      <div className="px-16 py-12">
        <EmptyState
          icon={
            <Search aria-hidden className="size-5.5 text-muted-foreground" />
          }
          heading="Search the catalogue"
          description="Look for a title, artist, or genre, and we'll bring back the closest matches."
        >
          <SuggestedSearchTerms terms={suggestions ?? []} />
        </EmptyState>
      </div>
    );
  }

  const { data, error } = await client.GET("/api/search", {
    params: { query: { q: query } },
  });

  if (error) {
    throw new Error("Search failed.");
  }

  const hasMatches = data.bestMatches.length > 0;

  return (
    <div className="flex flex-col gap-8 px-16 py-12">
      <div className="flex flex-wrap items-baseline justify-between gap-2 border-b border-line pb-6">
        <h1 className="font-serif text-3xl font-medium italic">
          Results for <span className="text-rust">&ldquo;{query}&rdquo;</span>
        </h1>
        <span className="text-sm font-semibold text-muted-foreground">
          {data.totalCount} {data.totalCount === 1 ? "record" : "records"} found
        </span>
      </div>

      <div aria-live="polite">
        {hasMatches ? (
          <div className="flex flex-col gap-10">
            <section>
              <h2 className="mb-4 text-xs font-bold tracking-wide text-muted-foreground uppercase">
                Best matches
              </h2>
              <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-4">
                {data.bestMatches.map((album) => (
                  <ProductCard key={album.sqid} album={album} />
                ))}
              </div>
            </section>

            {data.suggestions.length > 0 && (
              <section>
                <h2 className="mb-1 text-xs font-bold tracking-wide text-muted-foreground uppercase">
                  You might also like
                </h2>
                <p className="mb-4 text-sm text-muted-foreground">
                  Similar mood and genre, even without an exact text match
                </p>
                <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-4">
                  {data.suggestions.map((album) => (
                    <ProductCard key={album.sqid} album={album} />
                  ))}
                </div>
              </section>
            )}
          </div>
        ) : (
          <EmptyState
            icon={
              <Search aria-hidden className="size-5.5 text-muted-foreground" />
            }
            heading="Nothing turned up"
            description={`No records matched "${query}". Try a different spelling, or one of these instead:`}
          >
            <SuggestedSearchTerms terms={data.suggestedTerms} />
          </EmptyState>
        )}
      </div>
    </div>
  );
}

function SearchSkeleton() {
  return (
    <LoadingState>
      <div className="px-16 pt-12 pb-8">
        <Skeleton className="h-8 w-64" />
      </div>
      <div className="px-16 pb-12">
        <SkeletonCardGrid />
      </div>
    </LoadingState>
  );
}

export default function SearchPage(props: PageProps<"/search">) {
  return (
    <Suspense fallback={<SearchSkeleton />}>
      <SearchContent searchParams={props.searchParams} />
    </Suspense>
  );
}
