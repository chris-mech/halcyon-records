import { describe, expect, test } from "vitest";

import { assertOk } from "./assert-ok";

describe("assertOk", () => {
  test("returns data when the result has no error", () => {
    const result = {
      data: { title: "Full Detail Album" },
      error: undefined,
      response: new Response(),
    };

    expect(assertOk(result, "Failed to load.")).toEqual({
      title: "Full Detail Album",
    });
  });

  test("throws with status and error as the cause when the result has an error", () => {
    const result = {
      data: undefined,
      error: { title: "Server Error" },
      response: new Response(null, { status: 503 }),
    };

    let caught: unknown;
    try {
      assertOk(result, "Failed to load.");
    } catch (error) {
      caught = error;
    }

    expect(caught).toBeInstanceOf(Error);
    expect((caught as Error).message).toBe("Failed to load.");
    expect((caught as Error).cause).toEqual({
      status: 503,
      error: { title: "Server Error" },
    });
  });
});
