import { Header } from "@/components/header";
import { Footer } from "@/components/footer";

export default function CheckoutLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <>
      <Header variant="stripped" backHref="/cart" backLabel="← Back to bag" />
      <main className="flex flex-1 flex-col">{children}</main>
      <Footer />
    </>
  );
}
