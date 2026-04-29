import { cn } from '@/lib/utils';

const statusColors = {
  // Application statuses
  Pending: 'bg-yellow-100 text-yellow-800',
  UnderReview: 'bg-blue-100 text-blue-800',
  Accepted: 'bg-green-100 text-green-800',
  Rejected: 'bg-red-100 text-red-800',
  Withdrawn: 'bg-slate-100 text-slate-800',
  
  // Athlete statuses
  Active: 'bg-green-100 text-green-800',
  Inactive: 'bg-slate-100 text-slate-800',
  OnHold: 'bg-yellow-100 text-yellow-800',
  Graduated: 'bg-purple-100 text-purple-800',
  Suspended: 'bg-red-100 text-red-800',
  
  // Program statuses
  Completed: 'bg-green-100 text-green-800',
  Paused: 'bg-yellow-100 text-yellow-800',
  Cancelled: 'bg-red-100 text-red-800',
} as const;

type StatusType = keyof typeof statusColors;

interface StatusBadgeProps {
  status: string;
  className?: string;
}

export function StatusBadge({ status, className }: StatusBadgeProps) {
  const colorClasses = statusColors[status as StatusType] || 'bg-slate-100 text-slate-800';
  
  return (
    <span
      className={cn(
        'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium',
        colorClasses,
        className
      )}
    >
      {status}
    </span>
  );
}
