import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Your Bag",
  description: "Review your bag before checkout.",
};

export default function CartLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return children;
}
