import { ALBUM_SORTS, type AlbumSort } from "@/lib/api/album-sort";

const DEFAULT_SORT: AlbumSort = "NewestFirst";

export const PAGE_SIZE = 12;

export interface ShopFilters {
  page: number;
  isNew: boolean;
  isOnSale: boolean;
  isStaffPick: boolean;
  genres: string[];
  sort: AlbumSort;
}

type RawSearchParams = Record<string, string | string[] | undefined>;

function firstValue(value: string | string[] | undefined): string | undefined {
  return Array.isArray(value) ? value[0] : value;
}

function parsePage(value: string | string[] | undefined): number {
  const parsed = Number(firstValue(value));
  return Number.isInteger(parsed) && parsed > 0 ? parsed : 1;
}

function parseBoolean(value: string | string[] | undefined): boolean {
  return firstValue(value) === "true";
}

function parseSort(value: string | string[] | undefined): AlbumSort {
  const raw = firstValue(value);
  return (ALBUM_SORTS as readonly string[]).includes(raw ?? "")
    ? (raw as AlbumSort)
    : DEFAULT_SORT;
}

function parseGenres(value: string | string[] | undefined): string[] {
  const raw = Array.isArray(value) ? value : value ? [value] : [];
  return raw.map((genre) => genre.trim()).filter((genre) => genre.length > 0);
}

export function parseShopFilters(searchParams: RawSearchParams): ShopFilters {
  return {
    page: parsePage(searchParams.page),
    isNew: parseBoolean(searchParams.isNew),
    isOnSale: parseBoolean(searchParams.isOnSale),
    isStaffPick: parseBoolean(searchParams.isStaffPick),
    genres: parseGenres(searchParams.genres),
    sort: parseSort(searchParams.sort),
  };
}

export function buildShopHref(
  current: ShopFilters,
  changes: Partial<ShopFilters>,
): string {
  const next: ShopFilters = { ...current, ...changes };
  if (!("page" in changes)) {
    next.page = 1;
  }

  const params = new URLSearchParams();
  if (next.page > 1) params.set("page", String(next.page));
  if (next.isNew) params.set("isNew", "true");
  if (next.isOnSale) params.set("isOnSale", "true");
  if (next.isStaffPick) params.set("isStaffPick", "true");
  for (const genre of next.genres) params.append("genres", genre);
  if (next.sort !== DEFAULT_SORT) params.set("sort", next.sort);

  const query = params.toString();
  return query ? `/shop?${query}` : "/shop";
}
