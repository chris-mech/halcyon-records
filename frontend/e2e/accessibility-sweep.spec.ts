import { expect, test } from "@playwright/test";

import { expectNoAccessibilityViolations } from "./axe-helper";

test.describe("accessibility sweep", () => {
  test("browse routes are free of accessibility violations", async ({
    page,
  }) => {
    await test.step("home", async () => {
      await page.goto("/");
      await expect(page.getByRole("heading", { level: 1 })).toBeVisible();
      await expectNoAccessibilityViolations(page);
    });

    await test.step("artists index and an artist detail page", async () => {
      await page.goto("/artists");
      await expect(
        page.getByRole("heading", { name: "Artists" }),
      ).toBeVisible();
      await expectNoAccessibilityViolations(page);

      await page.locator('a[href^="/artists/"]').first().click();
      await expect(page.getByRole("heading", { level: 1 })).toBeVisible();
      await expectNoAccessibilityViolations(page);
    });

    await test.step("genres index and a genre detail page", async () => {
      await page.goto("/genres");
      await expect(page.getByRole("heading", { name: "Genres" })).toBeVisible();
      await expectNoAccessibilityViolations(page);

      await page.locator('a[href^="/genres/"]').first().click();
      await expect(page.getByRole("heading", { level: 1 })).toBeVisible();
      await expectNoAccessibilityViolations(page);
    });

    await test.step("decades index and a decade detail page", async () => {
      await page.goto("/decades");
      await expect(
        page.getByRole("heading", { name: "Browse by decade" }),
      ).toBeVisible();
      await expectNoAccessibilityViolations(page);

      await page.locator('a[href^="/decades/"]').first().click();
      await expect(page.getByRole("heading", { level: 1 })).toBeVisible();
      await expectNoAccessibilityViolations(page);
    });

    await test.step("an album detail page", async () => {
      await page.goto("/shop");

      const firstCard = page.locator('[data-slot="card"]').first();
      await firstCard.locator('a[href^="/albums/"]').last().click();

      await expect(page.getByRole("heading", { level: 1 })).toBeVisible();
      await expectNoAccessibilityViolations(page);
    });
  });

  test("account details page is free of accessibility violations", async ({
    page,
  }) => {
    const email = `e2e-accessibility-sweep-${Date.now()}@example.test`;
    const password = "E2eTest1!";

    await page.goto("/register");
    await page.getByLabel("First name").fill("E2E");
    await page.getByLabel("Last name").fill("Accessibility Sweep");
    await page.getByLabel("Email").fill(email);
    await page.getByLabel("Password", { exact: true }).fill(password);
    await page.getByLabel("Confirm password").fill(password);
    await page.getByRole("button", { name: "Create account" }).click();

    await expect(page.getByRole("link", { name: "E2E" })).toBeVisible();

    await page.goto("/account/details");
    await expect(page.getByText("Member since")).toBeVisible();
    await expectNoAccessibilityViolations(page);
  });
});
