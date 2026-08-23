import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { SkipLink } from "@/components/skip-link";

export default function ShopLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <>
      <SkipLink />
      <Header />
      <main id="main-content" tabIndex={-1} className="flex flex-1 flex-col">
        {children}
      </main>
      <Footer />
    </>
  );
}
