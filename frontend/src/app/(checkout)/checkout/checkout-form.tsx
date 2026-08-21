"use client";

import { useRef } from "react";
import Image from "next/image";
import { useRouter } from "next/navigation";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useSession } from "next-auth/react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { formatPrice } from "@/lib/format";
import { syncCart } from "@/lib/cart/sync-cart";
import { useCartStore } from "@/lib/cart/cart-store";

import {
  checkoutContactSchema,
  type CheckoutContactFormValues,
} from "./checkout-schema";

const fieldLabelClassName =
  "text-[0.6875rem] font-extrabold tracking-[0.08em] text-muted-foreground uppercase";

function CheckoutForm() {
  const router = useRouter();
  const { data: session } = useSession();
  const items = useCartStore((state) => state.items);
  const idempotencyKeyRef = useRef<string | null>(null);

  const subtotalInPence = items.reduce(
    (total, item) => total + item.priceInPence * item.quantity,
    0,
  );

  const form = useForm<CheckoutContactFormValues>({
    resolver: zodResolver(checkoutContactSchema),
    defaultValues: {
      contactFirstName: session?.user.firstName ?? "",
      contactLastName: session?.user.lastName ?? "",
      contactEmail: session?.user.email ?? "",
    },
  });

  async function onSubmit(values: CheckoutContactFormValues) {
    idempotencyKeyRef.current ??= crypto.randomUUID();

    if (!(await syncCart())) {
      form.setError("root.serverError", {
        message: "Couldn't refresh your bag. Please try again.",
      });
      return;
    }

    const response = await fetch("/api/orders", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        contactFirstName: values.contactFirstName,
        contactLastName: values.contactLastName,
        contactEmail: values.contactEmail,
        idempotencyKey: idempotencyKeyRef.current,
      }),
    });

    if (!response.ok) {
      if (response.status === 409) {
        const body = await response.json();
        form.setError("root.serverError", {
          message: body.detail ?? "Sorry, an item in your bag just sold out.",
        });
        await syncCart();
        return;
      }

      form.setError("root.serverError", {
        message: "Something went wrong placing your order. Please try again.",
      });
      return;
    }

    const order = await response.json();
    useCartStore.getState().setItems([]);
    router.push(
      `/checkout/confirmation?order=${encodeURIComponent(order.orderNumber)}`,
    );
  }

  return (
    <div className="mx-auto w-full max-w-275 px-16 py-9 pb-25">
      <form
        onSubmit={(event) => {
          void form.handleSubmit(onSubmit)(event);
        }}
        method="post"
        noValidate
        className="grid grid-cols-[1.4fr_1fr] items-start gap-14"
      >
        <div className="flex flex-col gap-9">
          <div>
            <h2 className="mb-5 font-serif text-xl font-medium italic">
              Contact
            </h2>
            <p className="mb-4 text-xs text-muted-foreground">
              Pulled from your account — edit if this order needs different
              details.
            </p>
            <FieldError errors={[form.formState.errors.root?.serverError]} />
            <div className="grid grid-cols-2 gap-3">
              <Controller
                name="contactFirstName"
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
                name="contactLastName"
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
              name="contactEmail"
              control={form.control}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid} className="mt-3">
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
          </div>

          <div className="border-l-2 border-slate bg-background p-3 text-[0.6875rem] leading-relaxed text-muted-foreground">
            This is a demo store — no real payment is collected. Placing an
            order just creates an order record so you can see the full flow
            through.
          </div>

          <div>
            <Button
              type="submit"
              disabled={form.formState.isSubmitting}
              className="w-full py-4 text-xs font-bold tracking-wide uppercase"
            >
              {form.formState.isSubmitting
                ? "Placing order…"
                : `Place order (demo — no charge) — ${formatPrice(subtotalInPence)}`}
            </Button>
            <p className="mt-3 text-center text-[0.6875rem] text-muted-foreground">
              This is a portfolio project. No payment is processed and no real
              order will ship.
            </p>
          </div>
        </div>

        <Card className="border-line">
          <CardHeader>
            <CardTitle className="font-serif text-xl font-medium italic">
              Order summary
            </CardTitle>
          </CardHeader>
          <CardContent className="gap-3.5">
            {items.map((item) => (
              <div key={item.albumSqid} className="flex items-center gap-3">
                <div className="relative size-12 shrink-0 bg-slate-muted/40">
                  {item.imageUrl && (
                    <Image
                      src={item.imageUrl}
                      alt=""
                      fill
                      sizes="48px"
                      className="object-cover"
                    />
                  )}
                </div>
                <div className="flex-1">
                  <p className="text-sm font-medium">{item.title}</p>
                  <p className="text-xs text-muted-foreground">
                    Qty {item.quantity}
                  </p>
                </div>
                <p className="text-sm font-semibold">
                  {formatPrice(item.priceInPence * item.quantity)}
                </p>
              </div>
            ))}
            <div className="mt-2 flex justify-between border-t border-line pt-4 text-sm text-muted-foreground">
              <span>Subtotal</span>
              <span>{formatPrice(subtotalInPence)}</span>
            </div>
            <div className="flex justify-between text-sm text-muted-foreground">
              <span>Shipping</span>
              <span>Not applicable — demo order</span>
            </div>
            <div className="flex justify-between border-t border-line pt-4 text-base font-bold">
              <span>Total</span>
              <span>{formatPrice(subtotalInPence)}</span>
            </div>
          </CardContent>
        </Card>
      </form>
    </div>
  );
}

export { CheckoutForm };
