import { expect, test } from "@playwright/test";

import { expectNoAccessibilityViolations } from "./axe-helper";

test.describe("register and login", () => {
  test("a new account can register, then log back in with the same credentials", async ({
    page,
  }) => {
    const email = `e2e-register-login-${Date.now()}@example.test`;
    const password = "E2eTest1!";
    const firstName = "E2E";
    const lastName = "Register Flow";

    await test.step("register a new account", async () => {
      await page.goto("/register");
      await expectNoAccessibilityViolations(page);

      await page.getByLabel("First name").fill(firstName);
      await page.getByLabel("Last name").fill(lastName);
      await page.getByLabel("Email").fill(email);
      await page.getByLabel("Password", { exact: true }).fill(password);
      await page.getByLabel("Confirm password").fill(password);

      await page.getByRole("button", { name: "Create account" }).click();

      await expect(page.getByRole("link", { name: firstName })).toBeVisible();
    });

    await test.step("log out", async () => {
      await page.getByRole("button", { name: "Log out" }).click();

      await expect(page.getByRole("link", { name: "Log in" })).toBeVisible();
    });

    await test.step("log back in with the same credentials", async () => {
      await page.goto("/login");
      await expectNoAccessibilityViolations(page);

      await page.getByLabel("Email").fill(email);
      await page.getByLabel("Password", { exact: true }).fill(password);

      await page.getByRole("button", { name: "Log in" }).click();

      await expect(page.getByRole("link", { name: firstName })).toBeVisible();
    });
  });
});
