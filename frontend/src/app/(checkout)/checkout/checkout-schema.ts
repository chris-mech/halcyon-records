import { z } from "zod";

export const checkoutContactSchema = z.object({
  contactFirstName: z
    .string()
    .min(1, "First name is required.")
    .max(100, "First name must be 100 characters or fewer."),
  contactLastName: z
    .string()
    .min(1, "Last name is required.")
    .max(100, "Last name must be 100 characters or fewer."),
  contactEmail: z
    .email("Enter a valid email address.")
    .max(256, "Email must be 256 characters or fewer."),
});

export type CheckoutContactFormValues = z.infer<typeof checkoutContactSchema>;
