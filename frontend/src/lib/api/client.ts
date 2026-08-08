import createClient from "openapi-fetch";

import type { paths } from "./schema";

const client = createClient<paths>({
  baseUrl: process.env.API_URL,
});

export { client };
