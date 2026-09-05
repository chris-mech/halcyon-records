"use client";

import type { VariantProps } from "class-variance-authority";

import { Button, buttonVariants } from "@/components/ui/button";
import { toast } from "@/components/ui/toast";
import { useCartStore } from "@/lib/cart/cart-store";

interface CartEligibleAlbum {
  sqid: string;
  title: string;
  titleSlug: string;
  imageUrl: string | null;
  priceInPence: number;
  originalPriceInPence: number | null;
  unitsInStock: number;
  isInStock: boolean;
  artists: { sqid: string; name: string; nameSlug: string }[];
}

interface AddToCartButtonProps {
  album: CartEligibleAlbum;
  quantity?: number;
  variant?: VariantProps<typeof buttonVariants>["variant"];
  size?: VariantProps<typeof buttonVariants>["size"];
  className?: string;
}

function AddToCartButton({
  album,
  quantity = 1,
  variant,
  size,
  className,
}: AddToCartButtonProps) {
  const inCartQuantity = useCartStore(
    (state) =>
      state.items.find((item) => item.albumSqid === album.sqid)?.quantity ?? 0,
  );
  const addItem = useCartStore((state) => state.addItem);

  const remaining = album.unitsInStock - inCartQuantity;
  const disabled = !album.isInStock || remaining <= 0;

  function handleClick() {
    addItem({
      albumSqid: album.sqid,
      title: album.title,
      titleSlug: album.titleSlug,
      imageUrl: album.imageUrl,
      priceInPence: album.priceInPence,
      originalPriceInPence: album.originalPriceInPence,
      quantity,
      unitsInStock: album.unitsInStock,
      isInStock: album.isInStock,
      artists: album.artists,
    });

    toast.add({ title: "Added to cart", description: album.title });
  }

  return (
    <Button
      type="button"
      variant={variant}
      size={size}
      disabled={disabled}
      onClick={handleClick}
      className={className}
    >
      Add to cart
    </Button>
  );
}

export { AddToCartButton };
export type { CartEligibleAlbum };
