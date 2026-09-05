"use client";

import { Suspense, useEffect, useRef, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { ChevronDown, Search, User, XIcon } from "lucide-react";
import { signOut, useSession } from "next-auth/react";

import { Wordmark } from "@/components/wordmark";
import { Button, buttonVariants } from "@/components/ui/button";
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

function HeaderSearch() {
  const searchParams = useSearchParams();
  const currentQuery = searchParams.get("q") ?? "";
  const [prevQuery, setPrevQuery] = useState(currentQuery);
  const [searchQuery, setSearchQuery] = useState(currentQuery);
  const searchInputRef = useRef<HTMLInputElement>(null);

  if (currentQuery !== prevQuery) {
    setPrevQuery(currentQuery);
    setSearchQuery(currentQuery);
  }

  function handleClearSearch() {
    setSearchQuery("");
    searchInputRef.current?.focus();
  }

  return (
    <form action="/search" className="relative max-w-85 flex-1">
      <label htmlFor="header-search" className="sr-only">
        Search artists, albums, genres
      </label>
      <Search
        aria-hidden
        className="pointer-events-none absolute top-1/2 left-3 size-3.5 -translate-y-1/2 text-paper/70"
      />
      <Input
        ref={searchInputRef}
        id="header-search"
        type="search"
        name="q"
        placeholder="Search artists, albums, genres…"
        value={searchQuery}
        onChange={(e) => setSearchQuery(e.target.value)}
        className="h-auto border-paper/25 bg-slate-plaque py-2.5 pr-9 pl-9 text-sm text-paper shadow-none placeholder:text-slate-muted [&::-webkit-search-cancel-button]:hidden"
      />
      {searchQuery && (
        <Button
          type="button"
          variant="ghost"
          size="icon-xs"
          aria-label="Clear search"
          onClick={handleClearSearch}
          className="absolute inset-y-0 right-2 my-auto text-paper/70 hover:bg-paper/10 hover:text-paper"
        >
          <XIcon aria-hidden />
        </Button>
      )}
    </form>
  );
}

function HeaderSearchFallback() {
  return (
    <div className="relative max-w-85 flex-1">
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
        placeholder="Search artists, albums, genres…"
        disabled
        className="h-auto border-paper/25 bg-slate-plaque py-2.5 pr-8 pl-9 text-sm text-paper shadow-none placeholder:text-slate-muted [&::-webkit-search-cancel-button]:hidden"
      />
    </div>
  );
}

function Header({
  variant = "full",
  backHref = "/",
  backLabel = "← Back home",
}: HeaderProps) {
  const { data: session, status } = useSession();
  const hydrated = useCartHydrated();
  const totalQuantity = useCartStore(selectCartTotalQuantity);
  const isSessionExpired = session?.error === "RefreshError";

  useEffect(() => {
    if (isSessionExpired) {
      void syncCartOnLogout().then(() => signOut());
    }
  }, [isSessionExpired]);

  return (
    <header className="flex items-center justify-between gap-8 bg-slate px-16 py-5">
      <Wordmark variant="header" />
      {variant === "full" ? (
        <>
          <Suspense fallback={<HeaderSearchFallback />}>
            <HeaderSearch />
          </Suspense>
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
              Cart ({hydrated ? totalQuantity : 0})
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
