import { cn } from "@/lib/utils";

const STEPS = [
  { key: "bag", label: "Bag" },
  { key: "login", label: "Log in" },
  { key: "checkout", label: "Checkout" },
  { key: "confirmation", label: "Confirmation" },
] as const;

type CheckoutStep = (typeof STEPS)[number]["key"];

interface StepIndicatorProps {
  currentStep: CheckoutStep;
}

function StepIndicator({ currentStep }: StepIndicatorProps) {
  const currentIndex = STEPS.findIndex((step) => step.key === currentStep);

  return (
    <nav
      aria-label="Checkout progress"
      className="flex gap-8 text-xs font-bold tracking-wider uppercase"
    >
      {STEPS.map((step, index) => {
        const isDone = index < currentIndex;
        const isCurrent = index === currentIndex;

        return (
          <span
            key={step.key}
            aria-current={isCurrent ? "step" : undefined}
            className={cn(
              "text-ink-muted",
              (isDone || isCurrent) && "text-ink",
              isCurrent && "border-b-2 border-rust pb-1.5",
            )}
          >
            {step.label}
            {isDone && <span className="text-rust"> ✓</span>}
          </span>
        );
      })}
    </nav>
  );
}

export { StepIndicator };
export type { CheckoutStep };
