import { Suspense } from "react";
import type { Metadata } from "next";

import { safeNextPath } from "@/lib/safe-next-path";
import { LoginForm } from "./login-form";

export const metadata: Metadata = {
  title: "Log In",
  description: "Log in to your Halcyon Records account.",
};

async function LoginPageContent({
  searchParams,
}: Pick<PageProps<"/login">, "searchParams">) {
  const { next } = await searchParams;

  return <LoginForm next={safeNextPath(next)} />;
}

export default function LoginPage(props: PageProps<"/login">) {
  return (
    <Suspense>
      <LoginPageContent searchParams={props.searchParams} />
    </Suspense>
  );
}
