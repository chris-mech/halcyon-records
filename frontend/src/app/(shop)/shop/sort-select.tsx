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

import { buildShopHref, type ShopFilters } from "./search-params";

interface SortSelectProps {
  filters: ShopFilters;
}

function SortSelect({ filters }: SortSelectProps) {
  const router = useRouter();

  return (
    <div className="flex items-center gap-2.5">
      <label
        id="sort-label"
        className="text-xs font-semibold tracking-wide text-muted-foreground uppercase"
      >
        Sort
      </label>
      <Select
        key={filters.sort}
        items={ALBUM_SORT_OPTIONS}
        defaultValue={filters.sort}
        onValueChange={(value) =>
          router.push(buildShopHref(filters, { sort: value as AlbumSort }))
        }
      >
        <SelectTrigger
          aria-labelledby="sort-label"
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

export { SortSelect };
