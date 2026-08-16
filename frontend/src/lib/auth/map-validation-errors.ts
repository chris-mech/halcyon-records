import type { UseFormSetError } from "react-hook-form";

import type { components } from "@/lib/api/schema";

import type { RegisterFormValues } from "./schemas";

export type RegisterError =
  | components["schemas"]["HttpValidationProblemDetails"]
  | components["schemas"]["DomainProblemDetails"];

type TargetField = keyof RegisterFormValues | "root.serverError";

const FIELD_BY_VALIDATION_KEY: Record<string, TargetField> = {
  FirstName: "firstName",
  LastName: "lastName",
  Email: "email",
  Password: "password",
};

const FIELD_BY_DOMAIN_CODE: Record<string, TargetField> = {
  "Auth.EmailAlreadyRegistered": "email",
};

function resolveValidationField(key: string): TargetField {
  if (key in FIELD_BY_VALIDATION_KEY) {
    return FIELD_BY_VALIDATION_KEY[key];
  }
  if (key.startsWith("Auth.Password")) {
    return "password";
  }
  return "root.serverError";
}

export function mapRegisterError(
  error: RegisterError,
  setError: UseFormSetError<RegisterFormValues>,
): void {
  if ("code" in error) {
    const field = FIELD_BY_DOMAIN_CODE[error.code] ?? "root.serverError";
    setError(field, {
      message: error.detail ?? "Something went wrong. Please try again.",
    });
    return;
  }

  const messagesByField = new Map<TargetField, string[]>();

  for (const [key, messages] of Object.entries(error.errors ?? {})) {
    const field = resolveValidationField(key);
    messagesByField.set(field, [
      ...(messagesByField.get(field) ?? []),
      ...messages,
    ]);
  }

  for (const [field, messages] of messagesByField) {
    setError(field, { message: messages.join(" ") });
  }
}
