import createClient from "openapi-fetch";
import { PHASE_PRODUCTION_BUILD } from "next/constants";

import type { paths } from "./schema";

function resolveApiBaseUrl(): string | undefined {
  const baseUrl = process.env.API_HTTPS;

  if (!baseUrl && process.env.NEXT_PHASE !== PHASE_PRODUCTION_BUILD) {
    throw new Error(
      "Missing API_HTTPS: the frontend must be run as an Aspire resource (WithReference(api)) for API service discovery to work.",
    );
  }

  return baseUrl;
}

const client = createClient<paths>({
  baseUrl: resolveApiBaseUrl(),
});

const REQUEST_TIMEOUT_MS = 10_000;

client.use({
  onRequest({ request }) {
    const timeoutSignal = AbortSignal.timeout(REQUEST_TIMEOUT_MS);
    return new Request(request, {
      signal: AbortSignal.any([request.signal, timeoutSignal]),
    });
  },
});

export { client };
