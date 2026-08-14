"use client";

import { useRouter } from "next/navigation";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ALBUM_SORT_OPTIONS, type AlbumSort } from "@/lib/api/album-sort";
import {
  buildCatalogHref,
  type CatalogFilters,
} from "@/lib/catalog-search-params";

interface CatalogSortSelectProps {
  basePath: string;
  filters: CatalogFilters;
}

function CatalogSortSelect({ basePath, filters }: CatalogSortSelectProps) {
  const router = useRouter();

  return (
    <div className="flex items-center gap-2.5">
      <label
        id="catalog-sort-label"
        className="text-xs font-semibold tracking-wide text-muted-foreground uppercase"
      >
        Sort
      </label>
      <Select
        key={filters.sort}
        items={ALBUM_SORT_OPTIONS}
        defaultValue={filters.sort}
        onValueChange={(value) =>
          router.push(
            buildCatalogHref(basePath, filters, { sort: value as AlbumSort }),
          )
        }
      >
        <SelectTrigger
          aria-labelledby="catalog-sort-label"
          className="text-xs font-semibold"
        >
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {ALBUM_SORT_OPTIONS.map(({ value, label }) => (
            <SelectItem key={value} value={value}>
              {label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}

export { CatalogSortSelect };
