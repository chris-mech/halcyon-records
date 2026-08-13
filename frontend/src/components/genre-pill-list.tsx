import Link from "next/link";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

interface Genre {
  name: string;
  slug: string;
}

interface GenrePillListProps {
  genres: Genre[];
  className?: string;
}

function GenrePillList({ genres, className }: GenrePillListProps) {
  if (genres.length === 0) {
    return null;
  }

  return (
    <div className={cn("flex flex-wrap gap-2", className)}>
      {genres.map((genre) => (
        <Link
          key={genre.slug}
          href={`/genres/${genre.slug}`}
          className={cn(
            buttonVariants({ variant: "outline" }),
            "h-auto w-fit border-ink px-2.75 py-1 text-[0.625rem] font-bold tracking-wide text-ink uppercase",
          )}
        >
          {genre.name}
        </Link>
      ))}
    </div>
  );
}

export { GenrePillList };
