import { SessionProvider } from "next-auth/react";

import { auth } from "@/auth";

async function AuthSessionProvider({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const session = await auth();

  return <SessionProvider session={session}>{children}</SessionProvider>;
}

export { AuthSessionProvider };
