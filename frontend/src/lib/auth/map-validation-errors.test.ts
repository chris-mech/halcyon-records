import { describe, expect, test, vi } from "vitest";

import { mapRegisterError } from "./map-validation-errors";

describe("mapRegisterError", () => {
  test("maps FluentValidation field keys to their RHF field names", () => {
    const setError = vi.fn();

    mapRegisterError(
      {
        type: null,
        title: "Validation Error",
        status: 400,
        detail: null,
        instance: null,
        errors: {
          FirstName: ["First name must not be empty."],
          Email: ["Email is not a valid email address."],
        },
      },
      setError,
    );

    expect(setError).toHaveBeenCalledWith("firstName", {
      message: "First name must not be empty.",
    });
    expect(setError).toHaveBeenCalledWith("email", {
      message: "Email is not a valid email address.",
    });
  });

  test("merges multiple Auth.Password* keys into a single password field error", () => {
    const setError = vi.fn();

    mapRegisterError(
      {
        type: null,
        title: "Validation Error",
        status: 400,
        detail: null,
        instance: null,
        errors: {
          "Auth.PasswordTooShort": ["Passwords must be at least 6 characters."],
          "Auth.PasswordRequiresDigit": [
            "Passwords must have at least one digit.",
          ],
        },
      },
      setError,
    );

    expect(setError).toHaveBeenCalledTimes(1);
    expect(setError).toHaveBeenCalledWith("password", {
      message:
        "Passwords must be at least 6 characters. Passwords must have at least one digit.",
    });
  });

  test("routes an unrecognised validation key to the root error", () => {
    const setError = vi.fn();

    mapRegisterError(
      {
        type: null,
        title: "Validation Error",
        status: 400,
        detail: null,
        instance: null,
        errors: { "Some.UnmappedKey": ["Something unexpected."] },
      },
      setError,
    );

    expect(setError).toHaveBeenCalledWith("root.serverError", {
      message: "Something unexpected.",
    });
  });

  test("maps the email-already-registered domain error to the email field", () => {
    const setError = vi.fn();

    mapRegisterError(
      {
        type: null,
        title: "Conflict",
        status: 409,
        detail: "An account with email 'taken@test.invalid' already exists.",
        instance: null,
        code: "Auth.EmailAlreadyRegistered",
      },
      setError,
    );

    expect(setError).toHaveBeenCalledWith("email", {
      message: "An account with email 'taken@test.invalid' already exists.",
    });
  });

  test("routes an unmapped domain error code to the root error", () => {
    const setError = vi.fn();

    mapRegisterError(
      {
        type: null,
        title: "Unauthorized",
        status: 401,
        detail: null,
        instance: null,
        code: "Auth.SomethingElse",
      },
      setError,
    );

    expect(setError).toHaveBeenCalledWith("root.serverError", {
      message: "Something went wrong. Please try again.",
    });
  });
});
