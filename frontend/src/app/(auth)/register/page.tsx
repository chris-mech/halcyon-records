import { Suspense } from "react";

import { RegisterForm } from "./register-form";

async function RegisterPageContent({
  searchParams,
}: Pick<PageProps<"/register">, "searchParams">) {
  const { next } = await searchParams;

  return <RegisterForm next={typeof next === "string" ? next : undefined} />;
}

export default function RegisterPage(props: PageProps<"/register">) {
  return (
    <Suspense>
      <RegisterPageContent searchParams={props.searchParams} />
    </Suspense>
  );
}
