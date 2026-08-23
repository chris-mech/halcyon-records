import { expect, test } from "@playwright/test";

test.describe("skip link", () => {
  test("Tab reveals the skip link, and activating it moves focus to main content", async ({
    page,
  }) => {
    await page.goto("/shop");

    await page.keyboard.press("Tab");

    const skipLink = page.getByRole("link", { name: "Skip to content" });
    await expect(skipLink).toBeFocused();
    await expect(skipLink).toBeVisible();

    await page.keyboard.press("Enter");

    await expect(page.locator("#main-content")).toBeFocused();
  });
});
