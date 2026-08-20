import { client } from "@/lib/api/client";
import { requireAccessToken } from "@/lib/auth/require-access-token";

import type { components } from "@/lib/api/schema";

type CreateOrderRequest = components["schemas"]["CreateOrderRequest"];

export async function POST(request: Request) {
  const accessToken = await requireAccessToken(request);

  if (!accessToken) {
    return Response.json({ detail: "Not authenticated." }, { status: 401 });
  }

  let body: CreateOrderRequest;
  try {
    body = await request.json();
  } catch {
    return Response.json(
      { detail: "Malformed request body." },
      { status: 400 },
    );
  }

  const { data, error, response } = await client.POST("/api/orders", {
    headers: { Authorization: `Bearer ${accessToken}` },
    body,
  });

  if (error) {
    return Response.json(error, { status: response.status });
  }

  return Response.json(data, { status: response.status });
}
