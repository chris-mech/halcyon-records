import { describe, expect, test, vi } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { signIn } from "next-auth/react";

import { mergeCartAtLogin, notifyCartSyncFailed } from "@/lib/cart/sync-cart";
import { RegisterForm } from "./register-form";
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

vi.mock("@/lib/cart/sync-cart", () => ({
  mergeCartAtLogin: vi.fn().mockResolvedValue(true),
  notifyCartSyncFailed: vi.fn(),
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

describe("RegisterForm", () => {
  test("submits via POST so a pre-hydration native submit can never leak credentials into the URL", () => {
    const { container } = render(<RegisterForm />);

    expect(container.querySelector("form")).toHaveAttribute("method", "post");
  });

  test("shows validation errors when submitted empty", async () => {
    render(<RegisterForm />);

    submit();

    expect(
      await screen.findByText("First name is required."),
    ).toBeInTheDocument();
    expect(registerAction).not.toHaveBeenCalled();
  });

  test("shows a mismatch error when the passwords don't match", async () => {
    render(<RegisterForm />);

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
    render(<RegisterForm />);

    fillValidForm();
    submit();

    expect(
      await screen.findByText("An account with this email already exists."),
    ).toBeInTheDocument();
    expect(signIn).not.toHaveBeenCalled();
  });

  test("signs in and navigates home on successful registration when no next path is given", async () => {
    vi.mocked(registerAction).mockResolvedValue({ success: true });
    vi.mocked(signIn).mockResolvedValue({
      error: undefined,
      code: undefined,
      status: 200,
      ok: true,
      url: "/",
    });
    render(<RegisterForm />);

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
    expect(mergeCartAtLogin).toHaveBeenCalled();
  });

  test("shows a toast when the cart sync fails, but still navigates home", async () => {
    vi.mocked(registerAction).mockResolvedValue({ success: true });
    vi.mocked(mergeCartAtLogin).mockResolvedValueOnce(false);
    vi.mocked(signIn).mockResolvedValue({
      error: undefined,
      code: undefined,
      status: 200,
      ok: true,
      url: "/",
    });
    render(<RegisterForm />);

    fillValidForm();
    submit();

    await waitFor(() => expect(push).toHaveBeenCalledWith("/"));
    expect(notifyCartSyncFailed).toHaveBeenCalled();
  });

  test("navigates to the given next path on successful registration", async () => {
    vi.mocked(registerAction).mockResolvedValue({ success: true });
    vi.mocked(signIn).mockResolvedValue({
      error: undefined,
      code: undefined,
      status: 200,
      ok: true,
      url: "/",
    });
    render(<RegisterForm next="/checkout" />);

    fillValidForm();
    submit();

    await waitFor(() => expect(push).toHaveBeenCalledWith("/checkout"));
  });

  test("redirects to login with the next path if sign-in fails right after registration", async () => {
    vi.mocked(registerAction).mockResolvedValue({ success: true });
    vi.mocked(signIn).mockResolvedValue({
      error: "CredentialsSignin",
      code: "InvalidCredentials",
      status: 401,
      ok: false,
      url: null,
    });
    render(<RegisterForm next="/checkout" />);

    fillValidForm();
    submit();

    await waitFor(() =>
      expect(push).toHaveBeenCalledWith("/login?next=%2Fcheckout"),
    );
  });

  test("carries the next path into the log-in link", () => {
    render(<RegisterForm next="/checkout" />);

    expect(screen.getByRole("link", { name: "Log in" })).toHaveAttribute(
      "href",
      "/login?next=%2Fcheckout",
    );
  });
});
