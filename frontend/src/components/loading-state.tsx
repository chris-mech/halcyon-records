import type { ReactNode } from "react";

interface LoadingStateProps {
  children: ReactNode;
}

function LoadingState({ children }: LoadingStateProps) {
  return (
    <div role="status" aria-live="polite" data-cold-start-skeleton="true">
      <span className="sr-only">Loading</span>
      <div aria-hidden="true">{children}</div>
    </div>
  );
}

export { LoadingState };
