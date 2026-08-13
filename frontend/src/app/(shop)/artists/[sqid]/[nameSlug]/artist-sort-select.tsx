"use client";

import { useRouter } from "next/navigation";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { buildArtistHref, type ArtistAlbumSort } from "./search-params";

interface ArtistSortSelectProps {
  sqid: string;
  nameSlug: string;
  sort: ArtistAlbumSort;
}

const SORT_OPTIONS: { value: ArtistAlbumSort; label: string }[] = [
  { value: "NewestFirst", label: "Newest first" },
  { value: "OldestFirst", label: "Oldest first" },
  { value: "PriceAsc", label: "Price: low to high" },
  { value: "PriceDesc", label: "Price: high to low" },
];

function ArtistSortSelect({ sqid, nameSlug, sort }: ArtistSortSelectProps) {
  const router = useRouter();

  return (
    <div className="flex items-center gap-2.5">
      <label
        id="artist-sort-label"
        className="text-xs font-semibold tracking-wide text-muted-foreground uppercase"
      >
        Sort
      </label>
      <Select
        key={sort}
        items={SORT_OPTIONS}
        defaultValue={sort}
        onValueChange={(value) =>
          router.push(buildArtistHref(sqid, nameSlug, value as ArtistAlbumSort))
        }
      >
        <SelectTrigger
          aria-labelledby="artist-sort-label"
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

export { ArtistSortSelect };
