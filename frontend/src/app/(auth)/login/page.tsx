import { Suspense } from "react";

import { safeNextPath } from "@/lib/safe-next-path";
import { LoginForm } from "./login-form";

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
