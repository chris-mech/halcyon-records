import createClient from "openapi-fetch";

import type { paths } from "./schema";

function resolveApiBaseUrl(): string {
  const baseUrl = process.env.API_HTTPS;

  if (!baseUrl) {
    throw new Error(
      "Missing API_HTTPS — the frontend must be run as an Aspire resource (WithReference(api)) for API service discovery to work.",
    );
  }

  return baseUrl;
}

const client = createClient<paths>({
  baseUrl: resolveApiBaseUrl(),
});

export { client };
