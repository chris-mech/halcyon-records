import { LoadingState } from "@/components/loading-state";
import { SkeletonCardGrid } from "@/components/skeleton-primitives";
import { Skeleton } from "@/components/ui/skeleton";

export default function Loading() {
  return (
    <div className="flex flex-col gap-8 px-16 py-12">
      <Skeleton className="h-8 w-48" />
      <LoadingState>
        <SkeletonCardGrid />
      </LoadingState>
    </div>
  );
}
