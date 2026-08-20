import { Suspense } from "react";

import { safeNextPath } from "@/lib/safe-next-path";
import { RegisterForm } from "./register-form";

async function RegisterPageContent({
  searchParams,
}: Pick<PageProps<"/register">, "searchParams">) {
  const { next } = await searchParams;

  return <RegisterForm next={safeNextPath(next)} />;
}

export default function RegisterPage(props: PageProps<"/register">) {
  return (
    <Suspense>
      <RegisterPageContent searchParams={props.searchParams} />
    </Suspense>
  );
}
