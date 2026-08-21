import { describe, expect, test } from "vitest";

import { safeNextPath } from "./safe-next-path";

describe("safeNextPath", () => {
  test("accepts a same-origin path", () => {
    expect(safeNextPath("/checkout")).toBe("/checkout");
  });

  test("rejects an absolute off-site URL", () => {
    expect(safeNextPath("https://evil.test/phish")).toBeUndefined();
  });

  test("rejects a protocol-relative URL", () => {
    expect(safeNextPath("//evil.test/phish")).toBeUndefined();
  });

  test("rejects undefined and array values", () => {
    expect(safeNextPath(undefined)).toBeUndefined();
    expect(safeNextPath(["/checkout", "/other"])).toBeUndefined();
  });
});
