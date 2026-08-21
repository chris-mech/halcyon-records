import { expect, type Page } from "@playwright/test";
import { AxeBuilder } from "@axe-core/playwright";

async function expectNoAccessibilityViolations(page: Page): Promise<void> {
  const results = await new AxeBuilder({ page })
    .withTags(["wcag2a", "wcag2aa", "wcag21a", "wcag21aa"])
    .analyze();

  const summary = results.violations
    .map((violation) => {
      const elementCount = violation.nodes.length;
      return `- [${violation.impact}] ${violation.id}: ${violation.help} (${elementCount} element${elementCount === 1 ? "" : "s"})\n  ${violation.helpUrl}`;
    })
    .join("\n");

  expect(results.violations, summary).toHaveLength(0);
}

export { expectNoAccessibilityViolations };
