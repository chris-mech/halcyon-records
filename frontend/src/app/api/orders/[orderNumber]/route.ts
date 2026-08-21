import { client } from "@/lib/api/client";
import { requireAccessToken } from "@/lib/auth/require-access-token";

export async function GET(
  request: Request,
  ctx: RouteContext<"/api/orders/[orderNumber]">,
) {
  const accessToken = await requireAccessToken(request);

  if (!accessToken) {
    return Response.json({ detail: "Not authenticated." }, { status: 401 });
  }

  const { orderNumber } = await ctx.params;

  const { data, error, response } = await client.GET(
    "/api/orders/{orderNumber}",
    {
      headers: { Authorization: `Bearer ${accessToken}` },
      params: { path: { orderNumber } },
    },
  );

  if (error) {
    return Response.json(error, { status: response.status });
  }

  return Response.json(data);
}
