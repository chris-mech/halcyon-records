const SITE_NAME = "Halcyon Records";
const SITE_DESCRIPTION =
  "A full-stack record shop demo: browse, buy, and check out a curated vinyl catalogue.";

export function resolveSiteUrl(): string {
  if (process.env.NEXT_PUBLIC_SITE_URL) {
    return process.env.NEXT_PUBLIC_SITE_URL;
  }

  if (process.env.NEXT_PUBLIC_VERCEL_PROJECT_PRODUCTION_URL) {
    return `https://${process.env.NEXT_PUBLIC_VERCEL_PROJECT_PRODUCTION_URL}`;
  }

  return "http://localhost:3000";
}

export function resolveDeploymentUrl(): string {
  if (
    process.env.NEXT_PUBLIC_VERCEL_ENV === "preview" &&
    process.env.NEXT_PUBLIC_VERCEL_URL
  ) {
    return `https://${process.env.NEXT_PUBLIC_VERCEL_URL}`;
  }

  return resolveSiteUrl();
}

const SITE_URL = resolveSiteUrl();
const DEPLOYMENT_URL = resolveDeploymentUrl();

export { SITE_NAME, SITE_URL, DEPLOYMENT_URL, SITE_DESCRIPTION };
