import { z } from "zod";

export const registerSchema = z.object({
  firstName: z
    .string()
    .min(1, "First name is required.")
    .max(100, "First name must be 100 characters or fewer."),
  lastName: z
    .string()
    .min(1, "Last name is required.")
    .max(100, "Last name must be 100 characters or fewer."),
  email: z
    .email("Enter a valid email address.")
    .max(256, "Email must be 256 characters or fewer."),
  password: z
    .string()
    .min(6, "Password must be at least 6 characters.")
    .regex(/[a-z]/, "Password must contain a lowercase letter.")
    .regex(/[A-Z]/, "Password must contain an uppercase letter.")
    .regex(/\d/, "Password must contain a digit.")
    .regex(
      /[^a-zA-Z0-9]/,
      "Password must contain a non-alphanumeric character.",
    ),
});

export type RegisterFormValues = z.infer<typeof registerSchema>;

export const loginSchema = z.object({
  email: z.email("Enter a valid email address."),
  password: z.string().min(1, "Password is required."),
});

export type LoginFormValues = z.infer<typeof loginSchema>;
