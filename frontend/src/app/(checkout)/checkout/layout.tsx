import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Checkout",
  description: "Review your order before you check out.",
};

export default function CheckoutSectionLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return children;
}
