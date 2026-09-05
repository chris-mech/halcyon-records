import { expect, test } from "@playwright/test";

import { expectNoAccessibilityViolations } from "./axe-helper";

test.describe("cart to checkout", () => {
  test("anonymous browse and add-to-cart survives login, then checkout completes and the order shows in history", async ({
    page,
  }) => {
    const email = `e2e-cart-to-checkout-${Date.now()}@example.test`;
    const password = "E2eTest1!";
    const firstName = "E2E";
    const lastName = "Cart Checkout";

    let albumTitle = "";
    let orderNumber = "";
    const main = page.getByRole("main");

    await test.step("register an account, then log back out to browse anonymously", async () => {
      await page.goto("/register");
      await expectNoAccessibilityViolations(page);

      await page.getByLabel("First name").fill(firstName);
      await page.getByLabel("Last name").fill(lastName);
      await page.getByLabel("Email").fill(email);
      await page.getByLabel("Password", { exact: true }).fill(password);
      await page.getByLabel("Confirm password").fill(password);
      await page.getByRole("button", { name: "Create account" }).click();

      await expect(page.getByRole("button", { name: firstName })).toBeVisible();

      await page.getByRole("button", { name: firstName }).click();
      await page.getByRole("menuitem", { name: "Log out" }).click();
      await expect(page.getByRole("link", { name: "Log in" })).toBeVisible();
    });

    await test.step("browse the catalogue anonymously and add an album to the cart", async () => {
      await page.goto("/shop");
      await expectNoAccessibilityViolations(page);

      const firstCard = page.locator('[data-slot="card"]').first();
      const titleLink = firstCard.locator('a[href^="/albums/"]').last();
      albumTitle = (await titleLink.textContent())?.trim() ?? "";
      expect(albumTitle).toBeTruthy();

      await firstCard.getByRole("button", { name: "Add to cart" }).click();
      await expect(page.getByRole("link", { name: "Cart (1)" })).toBeVisible();
    });

    await test.step("the cart holds the item while still anonymous", async () => {
      await page.getByRole("link", { name: "Cart (1)" }).click();
      await expectNoAccessibilityViolations(page);

      await expect(
        page.getByRole("link", { name: albumTitle, exact: true }),
      ).toBeVisible();
    });

    await test.step("checkout requires logging in first", async () => {
      await page.getByRole("link", { name: "Checkout" }).click();

      await expect(page.getByText("Log in to check out")).toBeVisible();
      await expect(
        page
          .getByRole("navigation", { name: "Checkout progress" })
          .getByText("Log in", { exact: true }),
      ).toHaveAttribute("aria-current", "step");

      await page.getByRole("link", { name: "Log in" }).click();
    });

    await test.step("logging back in merges the anonymous cart (local wins)", async () => {
      await expectNoAccessibilityViolations(page);

      await page.getByLabel("Email").fill(email);
      await page.getByLabel("Password", { exact: true }).fill(password);
      await page.getByRole("button", { name: "Log in" }).click();

      await expect(
        page
          .getByRole("navigation", { name: "Checkout progress" })
          .getByText("Checkout", { exact: true }),
      ).toHaveAttribute("aria-current", "step");
      await expect(main.locator("p", { hasText: albumTitle })).toBeVisible();
    });

    await test.step("place the order", async () => {
      await expectNoAccessibilityViolations(page);

      await page.getByRole("button", { name: /^Place order/ }).click();

      await expect(page).toHaveURL(/\/checkout\/confirmation\?order=/);
      orderNumber = new URL(page.url()).searchParams.get("order") ?? "";
      expect(orderNumber).toBeTruthy();

      await expect(
        page.getByRole("heading", { name: "Order confirmed" }),
      ).toBeVisible();
      await expectNoAccessibilityViolations(page);

      await expect(main.locator("p", { hasText: albumTitle })).toBeVisible();
    });

    await test.step("the order shows up in order history", async () => {
      await page.goto("/account");
      await expectNoAccessibilityViolations(page);

      await expect(page.getByRole("link", { name: "Cart (0)" })).toBeVisible();

      await expect(page.getByText(`Order ${orderNumber}`)).toBeVisible();

      await page.getByRole("link", { name: "View order" }).click();
      await expectNoAccessibilityViolations(page);

      await expect(
        page.getByRole("heading", { name: `Order ${orderNumber}` }),
      ).toBeVisible();
      await expect(main.locator("p", { hasText: albumTitle })).toBeVisible();
    });
  });
});
