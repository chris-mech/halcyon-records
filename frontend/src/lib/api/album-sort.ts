import type { operations } from "@/lib/api/schema";

type GetAlbumsQuery = NonNullable<
  operations["GetAlbums"]["parameters"]["query"]
>;
type GetArtistByIdQuery = NonNullable<
  operations["GetArtistById"]["parameters"]["query"]
>;

export type AlbumSort = NonNullable<GetAlbumsQuery["sort"]>;
export type ArtistAlbumSort = NonNullable<GetArtistByIdQuery["sort"]>;

export const ALBUM_SORTS: readonly AlbumSort[] = [
  "NewestFirst",
  "OldestFirst",
  "PriceAsc",
  "PriceDesc",
  "ArtistAZ",
  "ArtistZA",
];

export const ALBUM_SORT_OPTIONS: { value: AlbumSort; label: string }[] = [
  { value: "NewestFirst", label: "Newest first" },
  { value: "OldestFirst", label: "Oldest first" },
  { value: "PriceAsc", label: "Price: low to high" },
  { value: "PriceDesc", label: "Price: high to low" },
  { value: "ArtistAZ", label: "Artist A–Z" },
  { value: "ArtistZA", label: "Artist Z–A" },
];

export const ARTIST_ALBUM_SORTS: readonly ArtistAlbumSort[] = [
  "NewestFirst",
  "OldestFirst",
  "PriceAsc",
  "PriceDesc",
];
