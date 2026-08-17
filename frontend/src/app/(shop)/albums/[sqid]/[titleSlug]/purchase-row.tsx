"use client";

import { useState } from "react";
import { Minus, Plus } from "lucide-react";

import { Button } from "@/components/ui/button";
import { AddToBagButton } from "@/components/add-to-bag-button";

import type { components } from "@/lib/api/schema";

type AlbumDetail = components["schemas"]["AlbumDetailResponse"];

interface PurchaseRowProps {
  album: AlbumDetail;
}

function PurchaseRow({ album }: PurchaseRowProps) {
  const [quantity, setQuantity] = useState(1);
  const isInStock = album.isInStock;
  const maxQuantity = album.unitsInStock;

  return (
    <div className="mb-9 flex items-center gap-4">
      <div className="flex items-center border border-line">
        <Button
          type="button"
          variant="ghost"
          disabled={!isInStock || quantity <= 1}
          onClick={() => setQuantity((current) => Math.max(1, current - 1))}
          className="h-11 w-9.5 rounded-none"
          aria-label="Decrease quantity"
        >
          <Minus aria-hidden className="size-3.5" />
        </Button>
        <span
          className="w-9 text-center text-sm font-semibold"
          aria-live="polite"
        >
          {quantity}
        </span>
        <Button
          type="button"
          variant="ghost"
          disabled={!isInStock || quantity >= maxQuantity}
          onClick={() =>
            setQuantity((current) => Math.min(maxQuantity, current + 1))
          }
          className="h-11 w-9.5 rounded-none"
          aria-label="Increase quantity"
        >
          <Plus aria-hidden className="size-3.5" />
        </Button>
      </div>
      <AddToBagButton
        album={album}
        quantity={quantity}
        className="h-11 flex-1 px-8.5 text-[0.8125rem] font-bold tracking-wide uppercase"
      />
    </div>
  );
}

export { PurchaseRow };