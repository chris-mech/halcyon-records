"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { signIn } from "next-auth/react";

import { mergeCartAtLogin, notifyCartSyncFailed } from "@/lib/cart/sync-cart";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { loginSchema, type LoginFormValues } from "@/lib/auth/schemas";

const SHOWCASE_ACCOUNT_EMAIL = "demo@halcyonrecords.example";
const SHOWCASE_ACCOUNT_PASSWORD = "DemoPassword123!";

const fieldLabelClassName =
  "text-[0.6875rem] font-extrabold tracking-[0.08em] text-muted-foreground uppercase";

interface LoginFormProps {
  next?: string;
}

function LoginForm({ next }: LoginFormProps) {
  const router = useRouter();
  const [isDemoSubmitting, setIsDemoSubmitting] = useState(false);
  const form = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });

  const registerHref = next
    ? `/register?next=${encodeURIComponent(next)}`
    : "/register";

  async function loginWith(email: string, password: string) {
    const result = await signIn("credentials", {
      email,
      password,
      redirect: false,
    });

    if (result.error) {
      form.setError("root.serverError", {
        message: "Invalid email or password.",
      });
      return;
    }

    if (!(await mergeCartAtLogin())) {
      notifyCartSyncFailed();
    }
    router.push(next ?? "/");
  }

  async function onSubmit(values: LoginFormValues) {
    await loginWith(values.email, values.password);
  }

  async function onDemoLogin() {
    setIsDemoSubmitting(true);
    await loginWith(SHOWCASE_ACCOUNT_EMAIL, SHOWCASE_ACCOUNT_PASSWORD);
    setIsDemoSubmitting(false);
  }

  return (
    <div className="flex flex-1 items-center justify-center px-16 py-16">
      <Card className="w-full max-w-105 border-border">
        <CardHeader>
          <CardTitle className="text-center font-serif text-2xl font-medium italic">
            Welcome back
          </CardTitle>
          <CardDescription className="text-center">
            Log in to see your orders and past picks
          </CardDescription>
        </CardHeader>
        <form
          onSubmit={form.handleSubmit(onSubmit)}
          method="post"
          noValidate
          className="flex flex-col gap-(--card-spacing)"
        >
          <CardContent className="flex flex-col gap-4">
            <FieldError errors={[form.formState.errors.root?.serverError]} />
            <Controller
              name="email"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid}>
                  <FieldLabel
                    htmlFor={field.name}
                    className={fieldLabelClassName}
                  >
                    Email
                  </FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    type="email"
                    autoComplete="email"
                    className="bg-background"
                    aria-invalid={fieldState.invalid}
                  />
                  <FieldError errors={[fieldState.error]} />
                </Field>
              )}
            />
            <Controller
              name="password"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid}>
                  <FieldLabel
                    htmlFor={field.name}
                    className={fieldLabelClassName}
                  >
                    Password
                  </FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    type="password"
                    autoComplete="current-password"
                    className="bg-background"
                    aria-invalid={fieldState.invalid}
                  />
                  <FieldError errors={[fieldState.error]} />
                </Field>
              )}
            />
          </CardContent>
          <CardFooter className="flex flex-col gap-4">
            <Button
              type="submit"
              disabled={form.formState.isSubmitting}
              className="w-full"
            >
              {form.formState.isSubmitting ? "Logging in…" : "Log in"}
            </Button>
            <Button
              type="button"
              variant="outline"
              disabled={isDemoSubmitting}
              onClick={onDemoLogin}
              className="w-full"
            >
              {isDemoSubmitting ? "Logging in…" : "Try the demo account"}
            </Button>
            <p className="text-center text-xs text-muted-foreground">
              This is a demo store: no real payments, nothing ships. This demo
              login is shared by every visitor, not private to you, so
              don&apos;t rely on anything you add to it sticking around. Sign in
              with {SHOWCASE_ACCOUNT_EMAIL} / {SHOWCASE_ACCOUNT_PASSWORD}, or
              the button above does it for you. Both real and demo accounts are
              cleared out periodically.
            </p>
            <p className="text-center text-sm text-muted-foreground">
              New here?{" "}
              <Link
                href={registerHref}
                className="font-bold text-ink underline underline-offset-4"
              >
                Create an account
              </Link>
            </p>
          </CardFooter>
        </form>
      </Card>
    </div>
  );
}

export { LoginForm };
