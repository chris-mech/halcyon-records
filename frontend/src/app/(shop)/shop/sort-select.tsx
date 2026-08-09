"use client";

import { useRouter } from "next/navigation";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import {
  buildShopHref,
  type AlbumSort,
  type ShopFilters,
} from "./search-params";

interface SortSelectProps {
  filters: ShopFilters;
}

const SORT_OPTIONS: { value: AlbumSort; label: string }[] = [
  { value: "NewestFirst", label: "Newest first" },
  { value: "OldestFirst", label: "Oldest first" },
  { value: "PriceAsc", label: "Price: low to high" },
  { value: "PriceDesc", label: "Price: high to low" },
  { value: "ArtistAZ", label: "Artist A–Z" },
  { value: "ArtistZA", label: "Artist Z–A" },
];

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
        items={SORT_OPTIONS}
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
          {SORT_OPTIONS.map(({ value, label }) => (
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
