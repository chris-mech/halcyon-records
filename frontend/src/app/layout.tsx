import type { Metadata, Viewport } from "next";
import { Big_Shoulders, Fraunces, Manrope } from "next/font/google";
import { SessionProvider } from "next-auth/react";
import { Suspense } from "react";
import "./globals.css";
import { SITE_DESCRIPTION, SITE_NAME, SITE_URL } from "@/lib/site-config";
import { cn } from "@/lib/utils";
import { AuthSessionProvider } from "@/components/auth-session-provider";
import { Toaster } from "@/components/ui/toast";

const manrope = Manrope({
  variable: "--font-manrope",
  subsets: ["latin"],
});

const fraunces = Fraunces({
  variable: "--font-fraunces",
  subsets: ["latin"],
  weight: ["500", "600"],
  style: ["italic", "normal"],
});

const bigShoulders = Big_Shoulders({
  variable: "--font-big-shoulders",
  subsets: ["latin"],
  weight: "variable",
  axes: ["opsz"],
});

export const metadata: Metadata = {
  metadataBase: new URL(SITE_URL),
  title: {
    default: SITE_NAME,
    template: `%s | ${SITE_NAME}`,
  },
  description: SITE_DESCRIPTION,
  openGraph: {
    siteName: SITE_NAME,
    type: "website",
    locale: "en_GB",
  },
};

export const viewport: Viewport = {
  colorScheme: "light",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html
      lang="en"
      className={cn(
        "h-full",
        "antialiased",
        manrope.variable,
        fraunces.variable,
        bigShoulders.variable,
        "font-sans",
      )}
    >
      <body className="flex min-h-full flex-col">
        <Suspense fallback={<SessionProvider>{children}</SessionProvider>}>
          <AuthSessionProvider>{children}</AuthSessionProvider>
        </Suspense>
        <Toaster />
      </body>
    </html>
  );
}
