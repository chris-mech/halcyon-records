"use client";

import Link from "next/link";
import { Minus, Plus } from "lucide-react";

import { InlineLinkList } from "@/components/inline-link-list";
import { MediaThumbnail } from "@/components/media-thumbnail";
import { Button } from "@/components/ui/button";
import { formatPrice } from "@/lib/format";
import { useCartStore } from "@/lib/cart/cart-store";
import type { CartItem } from "@/lib/cart/cart-store";

interface CartRowProps {
  item: CartItem;
}

function CartRow({ item }: CartRowProps) {
  const setItemQuantity = useCartStore((state) => state.setItemQuantity);
  const removeItem = useCartStore((state) => state.removeItem);

  const albumHref = `/albums/${item.albumSqid}/${item.titleSlug}`;

  return (
    <div className="flex items-center gap-5 border-b border-line py-6">
      <MediaThumbnail
        imageUrl={item.imageUrl}
        href={albumHref}
        sizes="80px"
        className="block size-20 shrink-0"
      />

      <div className="flex-1">
        <p className="mb-1 flex flex-wrap text-[0.6875rem] font-bold tracking-wide text-muted-foreground uppercase">
          <InlineLinkList
            items={item.artists.map((artist) => ({
              id: artist.sqid,
              href: `/artists/${artist.sqid}/${artist.nameSlug}`,
              label: artist.name,
            }))}
            linkClassName="hover:underline"
          />
        </p>
        <Link
          href={albumHref}
          className="mb-2 block font-serif text-lg italic hover:underline"
        >
          {item.title}
        </Link>
        <Button
          type="button"
          variant="link"
          onClick={() => removeItem(item.albumSqid)}
          className="h-auto p-0 text-[0.6875rem] font-semibold text-muted-foreground underline"
        >
          Remove
        </Button>
      </div>

      <div className="flex shrink-0 items-center gap-6">
        <div className="flex items-center border border-line">
          <Button
            type="button"
            variant="ghost"
            disabled={item.quantity <= 1}
            onClick={() => setItemQuantity(item.albumSqid, item.quantity - 1)}
            className="h-9 w-8 rounded-none"
            aria-label="Decrease quantity"
          >
            <Minus aria-hidden className="size-3.5" />
          </Button>
          <span
            className="w-7.5 text-center text-sm font-semibold"
            aria-live="polite"
          >
            {item.quantity}
          </span>
          <Button
            type="button"
            variant="ghost"
            disabled={!item.isInStock || item.quantity >= item.unitsInStock}
            onClick={() => setItemQuantity(item.albumSqid, item.quantity + 1)}
            className="h-9 w-8 rounded-none"
            aria-label="Increase quantity"
          >
            <Plus aria-hidden className="size-3.5" />
          </Button>
        </div>
        <p className="w-16 text-right text-sm font-bold">
          {formatPrice(item.priceInPence * item.quantity)}
        </p>
      </div>
    </div>
  );
}

export { CartRow };
