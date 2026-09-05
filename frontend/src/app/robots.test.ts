import { describe, expect, test } from "vitest";

import { SITE_URL } from "@/lib/site-config";

import robots from "./robots";

describe("robots", () => {
  test("disallows every crawler by default", () => {
    const { rules } = robots();

    expect(rules).toContainEqual({ userAgent: "*", disallow: "/" });
  });

  test("allows known social preview bots through, so link sharing still works", () => {
    const { rules } = robots();

    expect(rules).toContainEqual({
      userAgent: [
        "facebookexternalhit",
        "Twitterbot",
        "LinkedInBot",
        "Slackbot",
        "Discordbot",
        "WhatsApp",
        "TelegramBot",
      ],
      allow: "/",
    });
  });

  test("points at the sitemap", () => {
    expect(robots().sitemap).toBe(`${SITE_URL}/sitemap.xml`);
  });
});
