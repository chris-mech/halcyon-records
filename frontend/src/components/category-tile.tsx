import Image from "next/image";
import Link from "next/link";
import { DiscAlbum } from "lucide-react";

interface CategoryTileProps {
  href: string;
  name: string;
  imageUrl: string | null;
  albumCount: number;
}

function CategoryTile({ href, name, imageUrl, albumCount }: CategoryTileProps) {
  return (
    <Link
      href={href}
      className="block border border-line bg-paper shadow-lg outline-none focus-visible:ring-3 focus-visible:ring-ring"
    >
      <div className="relative aspect-[4/3] border-b border-line bg-slate-muted/40">
        {imageUrl ? (
          <Image
            src={imageUrl}
            alt=""
            fill
            sizes="(min-width: 1024px) 25vw, (min-width: 640px) 33vw, 50vw"
            className="object-cover"
          />
        ) : (
          <div className="flex size-full items-center justify-center">
            <DiscAlbum aria-hidden className="size-10 text-slate-muted" />
          </div>
        )}
      </div>
      <div className="p-4.5">
        <div className="mb-2 font-serif text-lg font-medium italic">{name}</div>
        <div className="text-xs font-semibold text-muted-foreground">
          {albumCount} {albumCount === 1 ? "record" : "records"}
        </div>
      </div>
    </Link>
  );
}

export { CategoryTile };
