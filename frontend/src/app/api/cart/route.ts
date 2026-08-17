import { client } from "@/lib/api/client";
import { requireAccessToken } from "@/lib/auth/require-access-token";

export async function GET(request: Request) {
  const accessToken = await requireAccessToken(request);

  if (!accessToken) {
    return Response.json({ detail: "Not authenticated." }, { status: 401 });
  }

  const { data, error, response } = await client.GET("/api/cart", {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  if (error) {
    return Response.json(error, { status: response.status });
  }

  return Response.json(data);
}
