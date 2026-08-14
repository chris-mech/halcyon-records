import { ALBUM_SORTS, type AlbumSort } from "@/lib/api/album-sort";

const DEFAULT_SORT: AlbumSort = "NewestFirst";

export const PAGE_SIZE = 12;

export interface CatalogFilters {
  page: number;
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

function parseSort(value: string | string[] | undefined): AlbumSort {
  const raw = firstValue(value);
  return (ALBUM_SORTS as readonly string[]).includes(raw ?? "")
    ? (raw as AlbumSort)
    : DEFAULT_SORT;
}

export function parseCatalogFilters(
  searchParams: RawSearchParams,
): CatalogFilters {
  return {
    page: parsePage(searchParams.page),
    sort: parseSort(searchParams.sort),
  };
}

export function buildCatalogHref(
  basePath: string,
  current: CatalogFilters,
  changes: Partial<CatalogFilters>,
): string {
  const next: CatalogFilters = { ...current, ...changes };
  if (!("page" in changes)) {
    next.page = 1;
  }

  const params = new URLSearchParams();
  if (next.page > 1) params.set("page", String(next.page));
  if (next.sort !== DEFAULT_SORT) params.set("sort", next.sort);

  const query = params.toString();
  return query ? `${basePath}?${query}` : basePath;
}
