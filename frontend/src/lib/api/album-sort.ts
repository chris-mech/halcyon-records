// TODO: remove hardcoded enum after openapi fix
export type AlbumSort =
  | "NewestFirst"
  | "OldestFirst"
  | "PriceAsc"
  | "PriceDesc"
  | "ArtistAZ"
  | "ArtistZA";

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
