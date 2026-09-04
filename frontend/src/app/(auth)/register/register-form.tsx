"use client";

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
import { mapRegisterError } from "@/lib/auth/map-validation-errors";
import { registerSchema, type RegisterFormValues } from "@/lib/auth/schemas";

import { registerAction } from "./actions";

const fieldLabelClassName =
  "text-[0.6875rem] font-extrabold tracking-[0.08em] text-muted-foreground uppercase";

interface RegisterFormProps {
  next?: string;
}

function RegisterForm({ next }: RegisterFormProps) {
  const router = useRouter();
  const form = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      firstName: "",
      lastName: "",
      email: "",
      password: "",
      confirmPassword: "",
    },
  });

  const loginHref = next ? `/login?next=${encodeURIComponent(next)}` : "/login";

  async function onSubmit(values: RegisterFormValues) {
    const result = await registerAction({
      firstName: values.firstName,
      lastName: values.lastName,
      email: values.email,
      password: values.password,
    });

    if (!result.success) {
      mapRegisterError(result.error, form.setError);
      return;
    }

    const signInResult = await signIn("credentials", {
      email: values.email,
      password: values.password,
      redirect: false,
    });

    if (signInResult.error) {
      router.push(loginHref);
      return;
    }

    if (!(await mergeCartAtLogin())) {
      notifyCartSyncFailed();
    }
    router.push(next ?? "/");
  }

  return (
    <div className="flex flex-1 items-center justify-center px-16 py-16">
      <Card className="w-full max-w-105 border-border">
        <CardHeader>
          <CardTitle className="text-center font-serif text-2xl font-medium italic">
            Create an account
          </CardTitle>
          <CardDescription className="text-center">
            Track orders and pick up where you left off
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
            <div className="grid grid-cols-2 gap-3">
              <Controller
                name="firstName"
                control={form.control}
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid}>
                    <FieldLabel
                      htmlFor={field.name}
                      className={fieldLabelClassName}
                    >
                      First name
                    </FieldLabel>
                    <Input
                      {...field}
                      id={field.name}
                      type="text"
                      autoComplete="given-name"
                      className="bg-background"
                      aria-invalid={fieldState.invalid}
                    />
                    <FieldError errors={[fieldState.error]} />
                  </Field>
                )}
              />
              <Controller
                name="lastName"
                control={form.control}
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid}>
                    <FieldLabel
                      htmlFor={field.name}
                      className={fieldLabelClassName}
                    >
                      Last name
                    </FieldLabel>
                    <Input
                      {...field}
                      id={field.name}
                      type="text"
                      autoComplete="family-name"
                      className="bg-background"
                      aria-invalid={fieldState.invalid}
                    />
                    <FieldError errors={[fieldState.error]} />
                  </Field>
                )}
              />
            </div>
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
                    autoComplete="new-password"
                    className="bg-background"
                    aria-invalid={fieldState.invalid}
                  />
                  <FieldError errors={[fieldState.error]} />
                </Field>
              )}
            />
            <Controller
              name="confirmPassword"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid}>
                  <FieldLabel
                    htmlFor={field.name}
                    className={fieldLabelClassName}
                  >
                    Confirm password
                  </FieldLabel>
                  <Input
                    {...field}
                    id={field.name}
                    type="password"
                    autoComplete="new-password"
                    className="bg-background"
                    aria-invalid={fieldState.invalid}
                  />
                  <FieldError errors={[fieldState.error]} />
                </Field>
              )}
            />
            <div className="border-l-2 border-slate bg-background p-3 text-[0.6875rem] leading-relaxed text-muted-foreground">
              This is a public demo project. Please don&apos;t use a real name,
              email, or password you use elsewhere. Demo accounts (and any
              orders on them) are periodically cleared out, so don&apos;t rely
              on this for anything permanent.
            </div>
          </CardContent>
          <CardFooter className="flex flex-col gap-4">
            <Button
              type="submit"
              disabled={form.formState.isSubmitting}
              className="w-full"
            >
              {form.formState.isSubmitting
                ? "Creating account…"
                : "Create account"}
            </Button>
            <p className="text-center text-sm text-muted-foreground">
              Already have an account?{" "}
              <Link
                href={loginHref}
                className="font-bold text-ink underline underline-offset-4"
              >
                Log in
              </Link>
            </p>
          </CardFooter>
        </form>
      </Card>
    </div>
  );
}

export { RegisterForm };
