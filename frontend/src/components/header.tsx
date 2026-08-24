"use client";

import { useEffect } from "react";
import Link from "next/link";
import { ChevronDown, Search, User } from "lucide-react";
import { signOut, useSession } from "next-auth/react";

import { Wordmark } from "@/components/wordmark";
import { buttonVariants } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  selectCartTotalQuantity,
  useCartHydrated,
  useCartStore,
} from "@/lib/cart/cart-store";
import { syncCartOnLogout } from "@/lib/cart/sync-cart";
import { cn } from "@/lib/utils";

const navLinks = [
  { href: "/shop", label: "Shop" },
  { href: "/genres", label: "Genres" },
  { href: "/decades", label: "Decades" },
  { href: "/artists", label: "Artists" },
];

interface HeaderProps {
  variant?: "full" | "stripped";
  backHref?: string;
  backLabel?: string;
}

function Header({
  variant = "full",
  backHref = "/",
  backLabel = "← Back home",
}: HeaderProps) {
  const { data: session, status } = useSession();
  const hydrated = useCartHydrated();
  const totalQuantity = useCartStore(selectCartTotalQuantity);

  useEffect(() => {
    if (session?.error === "RefreshError") {
      void syncCartOnLogout().then(() => signOut());
    }
  }, [session?.error]);

  return (
    <header className="flex items-center justify-between gap-8 bg-slate px-16 py-5">
      <Wordmark variant="header" />
      {variant === "full" ? (
        <>
          <form action="/search" className="relative max-w-85 flex-1">
            <label htmlFor="header-search" className="sr-only">
              Search artists, albums, genres
            </label>
            <Search
              aria-hidden
              className="pointer-events-none absolute top-1/2 left-3 size-3.5 -translate-y-1/2 text-paper/70"
            />
            <Input
              id="header-search"
              type="search"
              name="q"
              placeholder="Search artists, albums, genres…"
              className="h-auto border-paper/25 bg-slate-plaque py-2.5 pr-4 pl-9 text-sm text-paper shadow-none placeholder:text-slate-muted"
            />
          </form>
          <nav className="shrink-0">
            <ul className="flex gap-7">
              {navLinks.map(({ href, label }) => (
                <li key={href}>
                  <Link
                    href={href}
                    className="text-sm font-semibold tracking-wide text-paper uppercase"
                  >
                    {label}
                  </Link>
                </li>
              ))}
            </ul>
          </nav>
          <div className="flex shrink-0 items-center gap-5">
            {status === "authenticated" && !isSessionExpired ? (
              <DropdownMenu>
                <DropdownMenuTrigger className="flex items-center gap-1.5 text-sm font-semibold text-paper outline-none hover:underline">
                  <User aria-hidden className="size-4" />
                  {session.user.firstName}
                  <ChevronDown aria-hidden className="size-3.5" />
                </DropdownMenuTrigger>
                <DropdownMenuContent>
                  <DropdownMenuItem render={<Link href="/account" />}>
                    Order history
                  </DropdownMenuItem>
                  <DropdownMenuItem render={<Link href="/account/details" />}>
                    Account details
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem
                    onClick={() =>
                      void syncCartOnLogout().then(() => signOut())
                    }
                  >
                    Log out
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            ) : (
              <Link
                href="/login"
                className="text-sm font-semibold text-slate-muted hover:text-paper"
              >
                Log in
              </Link>
            )}
            <Link
              href="/cart"
              className={cn(
                buttonVariants(),
                "h-auto px-4.5 py-2.25 text-xs font-bold tracking-wide uppercase",
              )}
            >
              Bag ({hydrated ? totalQuantity : 0})
            </Link>
          </div>
        </>
      ) : (
        <Link
          href={backHref}
          className="text-xs font-semibold tracking-wide text-paper uppercase"
        >
          {backLabel}
        </Link>
      )}
    </header>
  );
}

export { Header };
