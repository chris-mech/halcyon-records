import { describe, expect, test, vi } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { signIn } from "next-auth/react";

import LoginPage from "./page";

const push = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push }),
}));

vi.mock("next-auth/react", () => ({
  signIn: vi.fn(),
}));

function submit() {
  fireEvent.click(screen.getByRole("button", { name: "Log in" }));
}

describe("LoginPage", () => {
  test("submits via POST so a pre-hydration native submit can never leak credentials into the URL", () => {
    const { container } = render(<LoginPage />);

    expect(container.querySelector("form")).toHaveAttribute("method", "post");
  });

  test("shows validation errors when submitted empty", async () => {
    render(<LoginPage />);

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
    render(<LoginPage />);

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

  test("navigates home on successful login", async () => {
    vi.mocked(signIn).mockResolvedValue({
      error: undefined,
      code: undefined,
      status: 200,
      ok: true,
      url: "/",
    });
    render(<LoginPage />);

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
  });
});
