"use server";

import { client } from "@/lib/api/client";
import type { components } from "@/lib/api/schema";
import type { RegisterError } from "@/lib/auth/map-validation-errors";

type RegisterInput = components["schemas"]["RegisterRequest"];

type RegisterActionResult =
  { success: true } | { success: false; error: RegisterError };

export async function registerAction(
  input: RegisterInput,
): Promise<RegisterActionResult> {
  const { error } = await client.POST("/api/auth/register", {
    body: input,
  });

  if (error) {
    return { success: false, error };
  }

  return { success: true };
}
