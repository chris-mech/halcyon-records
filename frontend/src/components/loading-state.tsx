"use client";

import { useEffect, useState } from "react";
import type { ReactNode } from "react";

import { cn } from "@/lib/utils";

const ESCALATION_DELAY_MS = 6000;
const ROTATION_INTERVAL_MS = 5000;
const FADE_DURATION_MS = 400;

const FIXED_MESSAGE =
  "Still loading, and thank you so much for taking a look at my store. Azure SQL, Meilisearch, and the API all scale to zero when idle, so the first request wakes the whole stack back up.";

const TECH_MESSAGES = [
  "Built with .NET 10 and vertical slice architecture: each feature owns its own path straight through to the database.",
  "Search runs on Meilisearch, self-hosted and typo-tolerant.",
  "The whole stack is orchestrated locally with .NET Aspire: API, database, search, and frontend, one command.",
  "Frontend is Next.js 16 with React Server Components and partial prerendering.",
  "Infrastructure as code with Azure Bicep, deployed through a GitHub Actions CI/CD pipeline on every merge.",
  "Type-safe error handling throughout, using the Result pattern instead of exceptions for expected failures.",
  "The frontend's API client is generated straight from the backend's OpenAPI spec: one contract, no drift between the two.",
];

function pickNextMessageIndex(currentIndex: number, poolSize: number) {
  if (poolSize <= 1) {
    return 0;
  }

  let next = currentIndex;
  while (next === currentIndex) {
    next = Math.floor(Math.random() * poolSize);
  }
  return next;
}

interface LoadingStateProps {
  children: ReactNode;
}

function LoadingState({ children }: LoadingStateProps) {
  const [stage, setStage] = useState<"hidden" | "fixed" | "rotating">("hidden");
  const [messageIndex, setMessageIndex] = useState(-1);
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    let rotateTimeout: ReturnType<typeof setTimeout>;
    let fadeTimeout: ReturnType<typeof setTimeout>;

    function scheduleNextRotation() {
      rotateTimeout = setTimeout(() => {
        setVisible(false);
        fadeTimeout = setTimeout(() => {
          setMessageIndex((current) =>
            pickNextMessageIndex(current, TECH_MESSAGES.length),
          );
          setStage("rotating");
          setVisible(true);
          scheduleNextRotation();
        }, FADE_DURATION_MS);
      }, ROTATION_INTERVAL_MS);
    }

    const escalateTimeout = setTimeout(() => {
      setStage("fixed");
      setVisible(true);
      scheduleNextRotation();
    }, ESCALATION_DELAY_MS);

    return () => {
      clearTimeout(escalateTimeout);
      clearTimeout(rotateTimeout);
      clearTimeout(fadeTimeout);
    };
  }, []);

  const message =
    stage === "fixed"
      ? FIXED_MESSAGE
      : stage === "rotating"
        ? TECH_MESSAGES[messageIndex]
        : null;

  return (
    <div role="status" aria-live="polite">
      <span className="sr-only">Loading</span>
      <div aria-hidden="true">{children}</div>
      {message && (
        <p
          aria-hidden="true"
          className={cn(
            "mt-6 text-center text-sm text-muted-foreground transition-opacity motion-reduce:transition-none",
            visible ? "opacity-100" : "opacity-0",
          )}
          style={{ transitionDuration: `${FADE_DURATION_MS}ms` }}
        >
          {message}
        </p>
      )}
    </div>
  );
}

export { LoadingState, pickNextMessageIndex };
