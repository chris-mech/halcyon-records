import createClient from "openapi-fetch";
import { PHASE_PRODUCTION_BUILD } from "next/constants";

import type { paths } from "./schema";

export function resolveApiBaseUrl(): string | undefined {
  const baseUrl = process.env.API_HTTPS ?? process.env.API_BASE_URL;

  if (!baseUrl && process.env.NEXT_PHASE !== PHASE_PRODUCTION_BUILD) {
    throw new Error(
      "Missing API_HTTPS or API_BASE_URL: run the frontend as an Aspire resource (WithReference(api)) for local dev/CI service discovery, or set API_BASE_URL manually outside Aspire (e.g. on Vercel).",
    );
  }

  return baseUrl;
}

const client = createClient<paths>({
  baseUrl: resolveApiBaseUrl(),
});

const REQUEST_TIMEOUT_MS = 150_000;

client.use({
  onRequest({ request }) {
    const timeoutSignal = AbortSignal.timeout(REQUEST_TIMEOUT_MS);
    return new Request(request, {
      signal: AbortSignal.any([request.signal, timeoutSignal]),
    });
  },
});

export { client };
