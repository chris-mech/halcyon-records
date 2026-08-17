"use client";

import { Fragment } from "react";
import Image from "next/image";
import Link from "next/link";
import { Minus, Plus } from "lucide-react";

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
      <Link
        href={albumHref}
        aria-hidden
        tabIndex={-1}
        className="relative block size-20 shrink-0 bg-slate-muted/40"
      >
        {item.imageUrl && (
          <Image
            src={item.imageUrl}
            alt=""
            fill
            sizes="80px"
            className="object-cover"
          />
        )}
      </Link>

      <div className="flex-1">
        <p className="mb-1 flex flex-wrap gap-x-1 text-[0.6875rem] font-bold tracking-wide text-muted-foreground uppercase">
          {item.artists.map((artist, index) => (
            <Fragment key={artist.sqid}>
              {index > 0 && ", "}
              <Link
                href={`/artists/${artist.sqid}/${artist.nameSlug}`}
                className="hover:underline"
              >
                {artist.name}
              </Link>
            </Fragment>
          ))}
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
