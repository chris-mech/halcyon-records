import { client } from "@/lib/api/client";
import { requireAccessToken } from "@/lib/auth/require-access-token";

import type { components } from "@/lib/api/schema";

type SyncCartRequest = components["schemas"]["SyncCartRequest"];

export async function POST(request: Request) {
  const accessToken = await requireAccessToken(request);

  if (!accessToken) {
    return Response.json({ detail: "Not authenticated." }, { status: 401 });
  }

  let body: SyncCartRequest;
  try {
    body = await request.json();
  } catch {
    return Response.json(
      { detail: "Malformed request body." },
      { status: 400 },
    );
  }

  const { error, response } = await client.POST("/api/cart/sync", {
    headers: { Authorization: `Bearer ${accessToken}` },
    body,
  });

  if (error) {
    return Response.json(error, { status: response.status });
  }

  return new Response(null, { status: response.status });
}
