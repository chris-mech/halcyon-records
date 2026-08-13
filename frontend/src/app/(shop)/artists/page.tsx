import { Suspense } from "react";
import Link from "next/link";
import { connection } from "next/server";
import { cacheLife } from "next/cache";

import { client } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";

import { LetterStrip } from "./letter-strip";
import {
  LETTERS,
  letterAnchorId,
  letterKeyFor,
  type LetterKey,
} from "./letters";

type ArtistListItem = components["schemas"]["ArtistListItemResponse"];

type ArtistsDataResult =
  { ok: true; artists: ArtistListItem[] } | { ok: false };

async function getArtistsData(): Promise<ArtistsDataResult> {
  "use cache";
  cacheLife({ stale: 60, revalidate: 60, expire: 300 });

  const { data, error } = await client.GET("/api/artists");

  if (error) {
    return { ok: false };
  }

  return { ok: true, artists: data };
}

function groupArtistsByLetter(
  artists: ArtistListItem[],
): Map<LetterKey, ArtistListItem[]> {
  const groups = new Map<LetterKey, ArtistListItem[]>();

  for (const artist of artists) {
    const key = letterKeyFor(artist.name);
    const group = groups.get(key);
    if (group) {
      group.push(artist);
    } else {
      groups.set(key, [artist]);
    }
  }

  return groups;
}

export async function ArtistsContent() {
  await connection();

  const result = await getArtistsData();

  if (!result.ok) {
    throw new Error("Failed to load artists.");
  }

  const groups = groupArtistsByLetter(result.artists);
  const letterGroups = LETTERS.flatMap((letter) => {
    const artists = groups.get(letter);
    return artists ? [{ letter, artists }] : [];
  });

  return (
    <>
      <div className="mx-auto flex w-full max-w-275 flex-col gap-3 px-16 pt-14 pb-8">
        <span className="text-[0.6875rem] font-extrabold tracking-[0.08em] text-muted-foreground uppercase">
          Index
        </span>
        <h1 className="font-serif text-4xl font-medium italic">Artists</h1>
        <p className="max-w-130 text-sm text-muted-foreground">
          Every artist we carry, filed the old-fashioned way.
        </p>
      </div>

      <div className="mx-auto w-full max-w-275 px-16">
        <LetterStrip
          letters={LETTERS}
          presentLetters={new Set(groups.keys())}
        />
      </div>

      <div className="mx-auto w-full max-w-275 px-16 pb-25">
        {letterGroups.map(({ letter, artists }) => (
          <section
            key={letter}
            id={letterAnchorId(letter)}
            className="pt-9 first:pt-0"
          >
            <h2 className="mb-1 font-heading text-xl font-extrabold text-muted-foreground">
              {letter}
            </h2>
            <div className="flex flex-col">
              {artists.map((artist) => (
                <Link
                  key={artist.sqid}
                  href={`/artists/${artist.sqid}/${artist.nameSlug}`}
                  className="flex items-baseline justify-between border-b border-line py-4"
                >
                  <span className="font-serif text-lg italic">
                    {artist.name}
                  </span>
                  <span className="text-xs font-semibold text-muted-foreground">
                    {artist.albumCount}{" "}
                    {artist.albumCount === 1 ? "album" : "albums"}
                  </span>
                </Link>
              ))}
            </div>
          </section>
        ))}
      </div>
    </>
  );
}

function ArtistsSkeleton() {
  return (
    <div className="flex flex-1 items-center justify-center px-16 py-20 text-sm text-muted-foreground">
      Loading…
    </div>
  );
}

export default function ArtistsPage() {
  return (
    <Suspense fallback={<ArtistsSkeleton />}>
      <ArtistsContent />
    </Suspense>
  );
}
