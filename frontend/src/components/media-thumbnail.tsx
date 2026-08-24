import Image from "next/image";
import Link from "next/link";
import { DiscAlbum } from "lucide-react";

import { cn } from "@/lib/utils";

interface MediaThumbnailProps {
  imageUrl: string | null | undefined;
  alt?: string;
  href?: string;
  sizes: string;
  className?: string;
  iconClassName?: string;
}

function MediaThumbnail({
  imageUrl,
  alt = "",
  href,
  sizes,
  className,
  iconClassName = "size-10",
}: MediaThumbnailProps) {
  const content = imageUrl ? (
    <Image
      src={imageUrl}
      alt={alt}
      fill
      sizes={sizes}
      className="object-cover"
    />
  ) : (
    <div className="flex size-full items-center justify-center">
      <DiscAlbum aria-hidden className={cn("text-slate", iconClassName)} />
    </div>
  );

  const wrapperClassName = cn("relative bg-slate/15", className);

  if (href) {
    return (
      <Link href={href} aria-hidden tabIndex={-1} className={wrapperClassName}>
        {content}
      </Link>
    );
  }

  return <div className={wrapperClassName}>{content}</div>;
}

export { MediaThumbnail };
