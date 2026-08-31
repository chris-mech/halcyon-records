import { afterEach, beforeEach, describe, expect, test, vi } from "vitest";
import { act, render, screen } from "@testing-library/react";

import { LoadingState, pickNextMessageIndex } from "./loading-state";

const ESCALATION_DELAY_MS = 6000;
const ROTATION_INTERVAL_MS = 5000;
const FADE_DURATION_MS = 400;

describe("LoadingState", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  test("renders children and an accessible loading status immediately", () => {
    render(
      <LoadingState>
        <span data-testid="shape" />
      </LoadingState>,
    );

    expect(screen.getByRole("status")).toBeInTheDocument();
    expect(screen.getByText("Loading")).toBeInTheDocument();
    expect(screen.getByTestId("shape")).toBeInTheDocument();
  });

  test("shows no message before the escalation delay", () => {
    const { container } = render(
      <LoadingState>
        <span />
      </LoadingState>,
    );

    expect(container.querySelector("p")).not.toBeInTheDocument();
  });

  test("shows the fixed cold-start explanation once the escalation delay elapses", () => {
    const { container } = render(
      <LoadingState>
        <span />
      </LoadingState>,
    );

    act(() => {
      vi.advanceTimersByTime(ESCALATION_DELAY_MS);
    });

    expect(container.querySelector("p")).toHaveTextContent(/scale to zero/i);
  });

  test("rotates to a trivia message after the fixed message's turn", () => {
    const { container } = render(
      <LoadingState>
        <span />
      </LoadingState>,
    );

    act(() => {
      vi.advanceTimersByTime(
        ESCALATION_DELAY_MS + ROTATION_INTERVAL_MS + FADE_DURATION_MS,
      );
    });

    expect(container.querySelector("p")).not.toHaveTextContent(
      /scale to zero/i,
    );
  });

  test("keeps rotating to a different message on each subsequent turn", () => {
    const { container } = render(
      <LoadingState>
        <span />
      </LoadingState>,
    );

    act(() => {
      vi.advanceTimersByTime(
        ESCALATION_DELAY_MS + ROTATION_INTERVAL_MS + FADE_DURATION_MS,
      );
    });
    const firstMessage = container.querySelector("p")?.textContent;

    act(() => {
      vi.advanceTimersByTime(ROTATION_INTERVAL_MS + FADE_DURATION_MS);
    });
    const secondMessage = container.querySelector("p")?.textContent;

    expect(secondMessage).not.toBe(firstMessage);
  });

  test("keeps the message out of the announced live region", () => {
    render(
      <LoadingState>
        <span />
      </LoadingState>,
    );

    act(() => {
      vi.advanceTimersByTime(ESCALATION_DELAY_MS);
    });

    const status = screen.getByRole("status");
    const message = status.querySelector("p");
    expect(message).toHaveAttribute("aria-hidden", "true");
  });
});

describe("pickNextMessageIndex", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  test("returns 0 for a single-entry pool instead of looping forever", () => {
    expect(pickNextMessageIndex(0, 1)).toBe(0);
  });

  test("retries until it finds an index different from the current one", () => {
    vi.spyOn(Math, "random").mockReturnValueOnce(0).mockReturnValueOnce(0.5);

    expect(pickNextMessageIndex(0, 3)).toBe(1);
    expect(Math.random).toHaveBeenCalledTimes(2);
  });
});
