import Link from "next/link";

import { Header } from "@/components/header";
import { Footer } from "@/components/footer";

export default function NotFound() {
  return (
    <>
      <Header />
      <main className="flex flex-1 flex-col">
        <div className="mx-auto max-w-160 px-16 pt-30 pb-35 text-center">
          <div className="mb-6 font-heading text-[7.5rem] leading-none font-black uppercase">
            404
          </div>
          <h1 className="mb-3.5 font-serif text-[1.625rem] font-medium italic">
            This side&apos;s blank
          </h1>
          <p className="mx-auto mb-10 max-w-100 text-sm leading-[1.7] text-muted">
            Whatever you were looking for isn&apos;t here — maybe it got filed
            under the wrong genre, or maybe it never existed at all.
          </p>
          <div className="flex justify-center gap-3.5">
            <Link
              href="/shop"
              className="bg-rust px-7.5 py-3.5 text-[0.8125rem] font-bold tracking-wide text-paper uppercase"
            >
              Back to shop
            </Link>
            <Link
              href="/"
              className="border border-ink px-7.5 py-3.5 text-[0.8125rem] font-bold tracking-wide text-ink uppercase"
            >
              Go home
            </Link>
          </div>
        </div>
      </main>
      <Footer />
    </>
  );
}
