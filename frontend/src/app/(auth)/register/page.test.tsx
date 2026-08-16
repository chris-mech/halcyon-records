import { describe, expect, test, vi } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { signIn } from "next-auth/react";

import RegisterPage from "./page";
import { registerAction } from "./actions";

const push = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push }),
}));

vi.mock("next-auth/react", () => ({
  signIn: vi.fn(),
}));

vi.mock("./actions", () => ({
  registerAction: vi.fn(),
}));

function fillField(label: string, value: string) {
  fireEvent.change(screen.getByLabelText(label), { target: { value } });
}

function submit() {
  fireEvent.click(screen.getByRole("button", { name: "Create account" }));
}

function fillValidForm() {
  fillField("First name", "Given Name Field");
  fillField("Last name", "Family Name Field");
  fillField("Email", "user@example.com");
  fillField("Password", "Str0ng!Pass");
  fillField("Confirm password", "Str0ng!Pass");
}

describe("RegisterPage", () => {
  test("shows validation errors when submitted empty", async () => {
    render(<RegisterPage />);

    submit();

    expect(
      await screen.findByText("First name is required."),
    ).toBeInTheDocument();
    expect(registerAction).not.toHaveBeenCalled();
  });

  test("shows a mismatch error when the passwords don't match", async () => {
    render(<RegisterPage />);

    fillField("First name", "Given Name Field");
    fillField("Last name", "Family Name Field");
    fillField("Email", "user@example.com");
    fillField("Password", "Str0ng!Pass");
    fillField("Confirm password", "Different!Pass1");
    submit();

    expect(
      await screen.findByText("Passwords must match."),
    ).toBeInTheDocument();
    expect(registerAction).not.toHaveBeenCalled();
  });

  test("maps a 409 email-taken error onto the email field", async () => {
    vi.mocked(registerAction).mockResolvedValue({
      success: false,
      error: {
        code: "Auth.EmailAlreadyRegistered",
        detail: "An account with this email already exists.",
      },
    });
    render(<RegisterPage />);

    fillValidForm();
    submit();

    expect(
      await screen.findByText("An account with this email already exists."),
    ).toBeInTheDocument();
    expect(signIn).not.toHaveBeenCalled();
  });

  test("signs in and navigates home on successful registration", async () => {
    vi.mocked(registerAction).mockResolvedValue({ success: true });
    vi.mocked(signIn).mockResolvedValue({
      error: undefined,
      code: undefined,
      status: 200,
      ok: true,
      url: "/",
    });
    render(<RegisterPage />);

    fillValidForm();
    submit();

    await waitFor(() => expect(push).toHaveBeenCalledWith("/"));
    expect(registerAction).toHaveBeenCalledWith({
      firstName: "Given Name Field",
      lastName: "Family Name Field",
      email: "user@example.com",
      password: "Str0ng!Pass",
    });
    expect(signIn).toHaveBeenCalledWith("credentials", {
      email: "user@example.com",
      password: "Str0ng!Pass",
      redirect: false,
    });
  });

  test("redirects to login if sign-in fails right after a successful registration", async () => {
    vi.mocked(registerAction).mockResolvedValue({ success: true });
    vi.mocked(signIn).mockResolvedValue({
      error: "CredentialsSignin",
      code: "InvalidCredentials",
      status: 401,
      ok: false,
      url: null,
    });
    render(<RegisterPage />);

    fillValidForm();
    submit();

    await waitFor(() => expect(push).toHaveBeenCalledWith("/login"));
  });
});
