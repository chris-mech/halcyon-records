import type { ReactNode } from "react";

interface EmptyStateProps {
  icon: ReactNode;
  heading: string;
  description: string;
  children?: ReactNode;
}

function EmptyState({ icon, heading, description, children }: EmptyStateProps) {
  return (
    <div className="mx-auto w-full max-w-120 px-16 py-20 text-center">
      <div className="mx-auto mb-7 flex size-14 items-center justify-center rounded-full border border-line">
        {icon}
      </div>
      <h2 className="mb-3 font-serif text-xl font-medium italic">{heading}</h2>
      <p className="mb-8 text-sm leading-[1.7] text-muted-foreground">
        {description}
      </p>
      {children}
    </div>
  );
}

export { EmptyState };
