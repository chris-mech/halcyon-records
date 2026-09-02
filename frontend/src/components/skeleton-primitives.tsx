import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";

interface SkeletonCardGridProps {
  count?: number;
}

function SkeletonCardGrid({ count = 8 }: SkeletonCardGridProps) {
  return (
    <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-4">
      {Array.from({ length: count }, (_, index) => (
        <div key={index} className="flex flex-col gap-2.5">
          <Skeleton className="aspect-square w-full" />
          <Skeleton className="h-3 w-2/3" />
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-1/3" />
        </div>
      ))}
    </div>
  );
}

interface SkeletonLinesProps {
  count?: number;
  className?: string;
}

function SkeletonLines({ count = 3, className }: SkeletonLinesProps) {
  return (
    <div className={cn("flex flex-col gap-3", className)}>
      {Array.from({ length: count }, (_, index) => (
        <Skeleton
          key={index}
          className={cn("h-4", index === count - 1 ? "w-2/3" : "w-full")}
        />
      ))}
    </div>
  );
}

export { SkeletonCardGrid, SkeletonLines };
