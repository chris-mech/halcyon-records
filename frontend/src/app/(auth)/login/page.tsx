import { Suspense } from "react";

import { LoginForm } from "./login-form";

async function LoginPageContent({
  searchParams,
}: Pick<PageProps<"/login">, "searchParams">) {
  const { next } = await searchParams;

  return <LoginForm next={typeof next === "string" ? next : undefined} />;
}

export default function LoginPage(props: PageProps<"/login">) {
  return (
    <Suspense>
      <LoginPageContent searchParams={props.searchParams} />
    </Suspense>
  );
}
