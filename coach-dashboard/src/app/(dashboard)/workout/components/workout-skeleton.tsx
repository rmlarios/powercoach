'use client';

import { Skeleton } from '@/components/ui/skeleton';

/**
 * Thin, barely-visible loading skeleton for the workout page.
 * Designed to feel "invisible" as per GuiaUI — no heavy pulsing.
 */
export function WorkoutSkeleton() {
  return (
    <div className="max-w-lg mx-auto animate-in fade-in duration-300">
      {/* Header skeleton */}
      <div className="px-2 pt-1 pb-3 space-y-2">
        <Skeleton className="h-5 w-16 rounded-full" />
        <Skeleton className="h-3 w-48" />
        <Skeleton className="h-11 w-full rounded-lg" />
      </div>

      {/* Exercise skeletons */}
      {[1, 2, 3].map((i) => (
        <div key={i} className="py-4 px-2 space-y-2">
          <Skeleton className="h-4 w-40" />
          <Skeleton className="h-3 w-28" />
          <div className="space-y-1 mt-3">
            {[1, 2, 3].map((j) => (
              <div key={j} className="grid grid-cols-[40px_1fr_1fr_60px_44px] gap-1">
                <Skeleton className="h-8 w-full" />
                <Skeleton className="h-8 w-full" />
                <Skeleton className="h-8 w-full" />
                <Skeleton className="h-8 w-full" />
                <Skeleton className="h-8 w-8 rounded-md" />
              </div>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}
