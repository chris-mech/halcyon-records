import { PHASE_PRODUCTION_BUILD } from "next/constants";
import { afterEach, beforeAll, describe, expect, it } from "vitest";

describe("resolveApiBaseUrl", () => {
  let resolveApiBaseUrl: () => string | undefined;

  beforeAll(async () => {
    process.env.API_HTTPS = "https://module-init-placeholder.test";
    ({ resolveApiBaseUrl } = await import("./client"));
  });

  afterEach(() => {
    delete process.env.API_HTTPS;
    delete process.env.API_BASE_URL;
    delete process.env.NEXT_PHASE;
  });

  it("prefers API_HTTPS over API_BASE_URL when both are set", () => {
    process.env.API_HTTPS = "https://aspire-injected.test";
    process.env.API_BASE_URL = "https://vercel-set.test";

    expect(resolveApiBaseUrl()).toBe("https://aspire-injected.test");
  });

  it("falls back to API_BASE_URL when API_HTTPS is absent", () => {
    process.env.API_BASE_URL = "https://vercel-set.test";

    expect(resolveApiBaseUrl()).toBe("https://vercel-set.test");
  });

  it("returns undefined during a production build with neither var set", () => {
    process.env.NEXT_PHASE = PHASE_PRODUCTION_BUILD;

    expect(resolveApiBaseUrl()).toBeUndefined();
  });

  it("throws outside a production build when neither var is set", () => {
    expect(() => resolveApiBaseUrl()).toThrow(/API_HTTPS or API_BASE_URL/);
  });
});
