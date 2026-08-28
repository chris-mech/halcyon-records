import { afterEach, describe, expect, it } from "vitest";

import { resolveDeploymentUrl, resolveSiteUrl } from "./site-config";

describe("resolveSiteUrl", () => {
  afterEach(() => {
    delete process.env.NEXT_PUBLIC_SITE_URL;
    delete process.env.NEXT_PUBLIC_VERCEL_PROJECT_PRODUCTION_URL;
  });

  it("prefers an explicit NEXT_PUBLIC_SITE_URL override", () => {
    process.env.NEXT_PUBLIC_SITE_URL = "https://explicit-override.test";
    process.env.NEXT_PUBLIC_VERCEL_PROJECT_PRODUCTION_URL =
      "vercel-production.test";

    expect(resolveSiteUrl()).toBe("https://explicit-override.test");
  });

  it("falls back to Vercel's production domain when no override is set", () => {
    process.env.NEXT_PUBLIC_VERCEL_PROJECT_PRODUCTION_URL =
      "vercel-production.test";

    expect(resolveSiteUrl()).toBe("https://vercel-production.test");
  });

  it("falls back to localhost outside Vercel", () => {
    expect(resolveSiteUrl()).toBe("http://localhost:3000");
  });
});

describe("resolveDeploymentUrl", () => {
  afterEach(() => {
    delete process.env.NEXT_PUBLIC_SITE_URL;
    delete process.env.NEXT_PUBLIC_VERCEL_PROJECT_PRODUCTION_URL;
    delete process.env.NEXT_PUBLIC_VERCEL_URL;
    delete process.env.NEXT_PUBLIC_VERCEL_ENV;
  });

  it("self-references the preview deployment's own domain", () => {
    process.env.NEXT_PUBLIC_VERCEL_ENV = "preview";
    process.env.NEXT_PUBLIC_VERCEL_URL = "vercel-preview-deployment.test";
    process.env.NEXT_PUBLIC_VERCEL_PROJECT_PRODUCTION_URL =
      "vercel-production.test";

    expect(resolveDeploymentUrl()).toBe(
      "https://vercel-preview-deployment.test",
    );
  });

  it("defers to the site URL in production, ignoring the raw deployment URL", () => {
    process.env.NEXT_PUBLIC_VERCEL_ENV = "production";
    process.env.NEXT_PUBLIC_VERCEL_URL = "vercel-preview-deployment.test";
    process.env.NEXT_PUBLIC_VERCEL_PROJECT_PRODUCTION_URL =
      "vercel-production.test";

    expect(resolveDeploymentUrl()).toBe("https://vercel-production.test");
  });

  it("defers to the site URL when no deployment URL is available", () => {
    process.env.NEXT_PUBLIC_VERCEL_ENV = "preview";

    expect(resolveDeploymentUrl()).toBe("http://localhost:3000");
  });
});
