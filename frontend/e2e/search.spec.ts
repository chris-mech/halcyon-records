import { expect, test } from "@playwright/test";

import { expectNoAccessibilityViolations } from "./axe-helper";

test.describe("search", () => {
  test("searching for a suggested term returns matching results", async ({
    page,
  }) => {
    await page.goto("/search");
    await expectNoAccessibilityViolations(page);

    await expect(
      page.getByRole("heading", { name: "Search the catalogue" }),
    ).toBeVisible();

    const firstSuggestion = page.getByRole("main").getByRole("link").first();
    await expect(firstSuggestion).toBeVisible();
    const term = (await firstSuggestion.textContent())?.trim();
    expect(term).toBeTruthy();

    const searchInput = page.getByLabel("Search artists, albums, genres");
    await searchInput.fill(term!);
    await searchInput.press("Enter");

    await expect(
      page.getByRole("heading", { name: /^Results for/ }),
    ).toBeVisible();
    await expect(
      page.getByRole("heading", { name: "Best matches" }),
    ).toBeVisible();
    await expectNoAccessibilityViolations(page);
  });
});
