import { describe, expect, test, vi, beforeEach } from "vitest";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { signOut, useSession } from "next-auth/react";
import { ReadonlyURLSearchParams, useSearchParams } from "next/navigation";

import { useCartStore } from "@/lib/cart/cart-store";

import { Header } from "./header";

vi.mock("next-auth/react", () => ({
  useSession: vi.fn(),
  signOut: vi.fn(),
}));

vi.mock("next/navigation", async (importOriginal) => ({
  ...(await importOriginal()),
  useSearchParams: vi.fn(),
}));

vi.mock("@/lib/cart/sync-cart", () => ({
  syncCartOnLogout: vi.fn().mockResolvedValue(undefined),
}));

beforeEach(() => {
  useCartStore.setState({ items: [] });
  localStorage.clear();
  vi.mocked(useSearchParams).mockReturnValue(new ReadonlyURLSearchParams());
});

describe("Header", () => {
  test("shows a Log in link when unauthenticated", () => {
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: "unauthenticated",
      update: vi.fn(),
    });

    render(<Header />);

    expect(screen.getByRole("link", { name: "Log in" })).toHaveAttribute(
      "href",
      "/login",
    );
  });

  test("shows the user's first name and lets them get to account and log out", async () => {
    vi.mocked(useSession).mockReturnValue({
      data: {
        user: {
          id: "11111111-1111-1111-1111-111111111111",
          firstName: "Given Name Session",
          lastName: "Family Name Session",
          email: "given-name-session@test.invalid",
        },
        expires: "2099-01-01T00:00:00.000Z",
      },
      status: "authenticated",
      update: vi.fn(),
    });

    render(<Header />);

    fireEvent.click(screen.getByRole("button", { name: "Given Name Session" }));

    expect(
      await screen.findByRole("menuitem", { name: "Order history" }),
    ).toHaveAttribute("href", "/account");
    expect(
      screen.getByRole("menuitem", { name: "Account details" }),
    ).toHaveAttribute("href", "/account/details");

    const logOut = screen.getByRole("menuitem", { name: "Log out" });
    fireEvent.pointerDown(logOut);
    fireEvent.click(logOut);

    await waitFor(() => expect(signOut).toHaveBeenCalled());
  });

  test("signs out automatically when the session carries a RefreshError", async () => {
    vi.mocked(useSession).mockReturnValue({
      data: {
        user: {
          id: "11111111-1111-1111-1111-111111111111",
          firstName: "Given Name Session",
          lastName: "Family Name Session",
          email: "given-name-session@test.invalid",
        },
        error: "RefreshError",
        expires: "2099-01-01T00:00:00.000Z",
      },
      status: "authenticated",
      update: vi.fn(),
    });

    render(<Header />);

    await waitFor(() => expect(signOut).toHaveBeenCalled());
  });

  test("shows the live bag count from the cart store", () => {
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: "unauthenticated",
      update: vi.fn(),
    });
    useCartStore.setState({
      items: [
        {
          albumSqid: "header-count-album",
          title: "Header Count Fixture Album",
          titleSlug: "header-count-fixture-album",
          imageUrl: null,
          priceInPence: 1500,
          originalPriceInPence: null,
          quantity: 2,
          unitsInStock: 5,
          isInStock: true,
          artists: [],
        },
      ],
    });

    render(<Header />);

    expect(screen.getByRole("link", { name: "Bag (2)" })).toBeInTheDocument();
  });

  test("shows a Back to shop link in the stripped variant by default", () => {
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: "unauthenticated",
      update: vi.fn(),
    });

    render(<Header variant="stripped" />);

    expect(screen.getByRole("link", { name: "← Back home" })).toHaveAttribute(
      "href",
      "/",
    );
  });

  test("shows a custom back link when backHref and backLabel are given", () => {
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: "unauthenticated",
      update: vi.fn(),
    });

    render(
      <Header variant="stripped" backHref="/cart" backLabel="← Back to bag" />,
    );

    expect(screen.getByRole("link", { name: "← Back to bag" })).toHaveAttribute(
      "href",
      "/cart",
    );
  });

  test("prefills the search box from the current URL", () => {
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: "unauthenticated",
      update: vi.fn(),
    });
    vi.mocked(useSearchParams).mockReturnValue(
      new ReadonlyURLSearchParams("q=Existing+Query"),
    );

    render(<Header />);

    expect(
      screen.getByRole("searchbox", {
        name: "Search artists, albums, genres",
      }),
    ).toHaveValue("Existing Query");
  });

  test("shows a clear button only once there is a query, and clearing empties and refocuses the box", () => {
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: "unauthenticated",
      update: vi.fn(),
    });

    render(<Header />);

    const searchInput = screen.getByRole("searchbox", {
      name: "Search artists, albums, genres",
    });
    expect(
      screen.queryByRole("button", { name: "Clear search" }),
    ).not.toBeInTheDocument();

    fireEvent.change(searchInput, { target: { value: "Rock" } });
    const clearButton = screen.getByRole("button", { name: "Clear search" });

    fireEvent.click(clearButton);

    expect(searchInput).toHaveValue("");
    expect(searchInput).toHaveFocus();
    expect(
      screen.queryByRole("button", { name: "Clear search" }),
    ).not.toBeInTheDocument();
  });

  test("updates the search box when the URL's query changes under it, e.g. a suggested-term link", () => {
    vi.mocked(useSession).mockReturnValue({
      data: null,
      status: "unauthenticated",
      update: vi.fn(),
    });
    vi.mocked(useSearchParams).mockReturnValue(
      new ReadonlyURLSearchParams("q=First"),
    );

    const { rerender } = render(<Header />);

    expect(
      screen.getByRole("searchbox", {
        name: "Search artists, albums, genres",
      }),
    ).toHaveValue("First");

    vi.mocked(useSearchParams).mockReturnValue(
      new ReadonlyURLSearchParams("q=Second"),
    );
    rerender(<Header />);

    expect(
      screen.getByRole("searchbox", {
        name: "Search artists, albums, genres",
      }),
    ).toHaveValue("Second");
  });
});
