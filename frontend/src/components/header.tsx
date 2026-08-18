"use client";

import { useEffect } from "react";
import Link from "next/link";
import { Search } from "lucide-react";
import { signOut, useSession } from "next-auth/react";

import { Wordmark } from "@/components/wordmark";
import { Button, buttonVariants } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
}

function Header({ variant = "full" }: HeaderProps) {
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
              className="h-auto border-paper/25 bg-paper/10 py-2.5 pr-4 pl-9 text-sm text-paper shadow-none placeholder:text-slate-muted"
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
            {status === "authenticated" ? (
              <>
                <span className="text-sm font-semibold text-paper">
                  {session.user.firstName}
                </span>
                <Button
                  variant="ghost"
                  onClick={() => void syncCartOnLogout().then(() => signOut())}
                  className="h-auto p-0 text-sm font-semibold text-slate-muted hover:bg-transparent hover:text-paper"
                >
                  Log out
                </Button>
              </>
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
          href="/shop"
          className="text-xs font-semibold tracking-wide text-paper uppercase"
        >
          ← Back to shop
        </Link>
      )}
    </header>
  );
}

export { Header };
