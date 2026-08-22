import { describe, expect, test, vi } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { signIn } from "next-auth/react";

import { mergeCartAtLogin } from "@/lib/cart/sync-cart";
import { LoginForm } from "./login-form";

const push = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push }),
}));

vi.mock("next-auth/react", () => ({
  signIn: vi.fn(),
}));

vi.mock("@/lib/cart/sync-cart", () => ({
  mergeCartAtLogin: vi.fn(),
}));

function submit() {
  fireEvent.click(screen.getByRole("button", { name: "Log in" }));
}

describe("LoginForm", () => {
  test("submits via POST so a pre-hydration native submit can never leak credentials into the URL", () => {
    const { container } = render(<LoginForm />);

    expect(container.querySelector("form")).toHaveAttribute("method", "post");
  });

  test("shows validation errors when submitted empty", async () => {
    render(<LoginForm />);

    submit();

    expect(
      await screen.findByText("Enter a valid email address."),
    ).toBeInTheDocument();
    expect(screen.getByText("Password is required.")).toBeInTheDocument();
    expect(signIn).not.toHaveBeenCalled();
  });

  test("shows a generic error banner on invalid credentials", async () => {
    vi.mocked(signIn).mockResolvedValue({
      error: "CredentialsSignin",
      code: "InvalidCredentials",
      status: 401,
      ok: false,
      url: null,
    });
    render(<LoginForm />);

    fireEvent.change(screen.getByLabelText("Email"), {
      target: { value: "user@example.com" },
    });
    fireEvent.change(screen.getByLabelText("Password"), {
      target: { value: "wrong-password" },
    });
    submit();

    expect(
      await screen.findByText("Invalid email or password."),
    ).toBeInTheDocument();
    expect(push).not.toHaveBeenCalled();
  });

  test("navigates home on successful login when no next path is given", async () => {
    vi.mocked(signIn).mockResolvedValue({
      error: undefined,
      code: undefined,
      status: 200,
      ok: true,
      url: "/",
    });
    render(<LoginForm />);

    fireEvent.change(screen.getByLabelText("Email"), {
      target: { value: "user@example.com" },
    });
    fireEvent.change(screen.getByLabelText("Password"), {
      target: { value: "correct-password" },
    });
    submit();

    await waitFor(() => expect(push).toHaveBeenCalledWith("/"));
    expect(signIn).toHaveBeenCalledWith("credentials", {
      email: "user@example.com",
      password: "correct-password",
      redirect: false,
    });
    expect(mergeCartAtLogin).toHaveBeenCalled();
  });

  test("navigates to the given next path on successful login", async () => {
    vi.mocked(signIn).mockResolvedValue({
      error: undefined,
      code: undefined,
      status: 200,
      ok: true,
      url: "/",
    });
    render(<LoginForm next="/checkout" />);

    fireEvent.change(screen.getByLabelText("Email"), {
      target: { value: "user@example.com" },
    });
    fireEvent.change(screen.getByLabelText("Password"), {
      target: { value: "correct-password" },
    });
    submit();

    await waitFor(() => expect(push).toHaveBeenCalledWith("/checkout"));
  });

  test("carries the next path into the create-account link", () => {
    render(<LoginForm next="/checkout" />);

    expect(
      screen.getByRole("link", { name: "Create an account" }),
    ).toHaveAttribute("href", "/register?next=%2Fcheckout");
  });

  test("logs in with the fixed demo credentials when the demo button is clicked", async () => {
    vi.mocked(signIn).mockResolvedValue({
      error: undefined,
      code: undefined,
      status: 200,
      ok: true,
      url: "/",
    });
    render(<LoginForm />);

    fireEvent.click(
      screen.getByRole("button", { name: "Try the demo account" }),
    );

    await waitFor(() => expect(push).toHaveBeenCalledWith("/"));
    expect(signIn).toHaveBeenCalledWith("credentials", {
      email: "demo@halcyonrecords.example",
      password: "DemoPassword123!",
      redirect: false,
    });
    expect(mergeCartAtLogin).toHaveBeenCalled();
  });
});
