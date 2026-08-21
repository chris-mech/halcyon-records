import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { NotFoundContent } from "@/components/not-found-content";

export default function NotFound() {
  return (
    <>
      <Header />
      <main className="flex flex-1 flex-col">
        <NotFoundContent />
      </main>
      <Footer />
    </>
  );
}
