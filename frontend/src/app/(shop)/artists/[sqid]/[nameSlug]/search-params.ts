import { ARTIST_ALBUM_SORTS, type ArtistAlbumSort } from "@/lib/api/album-sort";

export type { ArtistAlbumSort };

const DEFAULT_SORT: ArtistAlbumSort = "NewestFirst";

type RawSearchParams = Record<string, string | string[] | undefined>;

function firstValue(value: string | string[] | undefined): string | undefined {
  return Array.isArray(value) ? value[0] : value;
}

export function parseArtistSort(
  searchParams: RawSearchParams,
): ArtistAlbumSort {
  const raw = firstValue(searchParams.sort);
  return (ARTIST_ALBUM_SORTS as readonly string[]).includes(raw ?? "")
    ? (raw as ArtistAlbumSort)
    : DEFAULT_SORT;
}

export function buildArtistHref(
  sqid: string,
  nameSlug: string,
  sort: ArtistAlbumSort,
): string {
  const path = `/artists/${sqid}/${nameSlug}`;
  return sort === DEFAULT_SORT ? path : `${path}?sort=${sort}`;
}
