import { Header } from "@/components/header";
import { Footer } from "@/components/footer";

export default function AuthLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <>
      <Header variant="stripped" />
      <main className="flex flex-1 flex-col">{children}</main>
      <Footer />
    </>
  );
}