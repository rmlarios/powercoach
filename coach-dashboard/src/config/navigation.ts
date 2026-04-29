import { 
  LayoutDashboard, 
  Users, 
  ClipboardList, 
  Dumbbell, 
  FileText,
  Settings,
  Timer,
  ShieldCheck,
  CheckSquare,
  LucideIcon,
  CalendarCheck,
  CreditCard,
  DollarSign,
  Package,
} from 'lucide-react';
import { routes } from './routes';

export type UserRole = 'Admin' | 'Coach' | 'Athlete';

export interface NavigationItem {
  title: string;
  href: string;
  icon: LucideIcon;
  badge?: string | number;
  children?: NavigationItem[];
}

/**
 * Coach navigation items
 */
const coachNavigation: NavigationItem[] = [
  {
    title: 'Dashboard',
    href: routes.dashboard,
    icon: LayoutDashboard,
  },
  {
    title: 'Applications',
    href: routes.applications,
    icon: ClipboardList,
  },
  {
    title: 'Athletes',
    href: routes.athletes,
    icon: Users,
  },
  {
    title: 'Check-ins',
    href: routes.checkIns,
    icon: CheckSquare,
  },
  {
    title: 'Programs',
    href: routes.programs,
    icon: FileText,
  },
  {
    title: 'Exercises',
    href: routes.exercises,
    icon: Dumbbell,
  },
  {
    title: 'Plans',
    href: routes.plans,
    icon: Package,
  },
  {
    title: 'Subscriptions',
    href: routes.subscriptions,
    icon: CreditCard,
  },
  {
    title: 'Payments',
    href: routes.payments,
    icon: DollarSign,
  },
  {
    title: 'Settings',
    href: routes.settings,
    icon: Settings,
  },
];

/**
 * Athlete navigation items
 */
const athleteNavigation: NavigationItem[] = [
  {
    title: 'Dashboard',
    href: routes.athleteDashboard,
    icon: LayoutDashboard,
  },
  {
    title: 'Workout',
    href: routes.workout,
    icon: Timer,
  },
  {
    title: 'Check-in',
    href: routes.athleteCheckIn,
    icon: CalendarCheck,
  },
  {
    title: 'Settings',
    href: routes.settings,
    icon: Settings,
  },
];

/**
 * Admin navigation items
 */
const adminNavigation: NavigationItem[] = [
  {
    title: 'Dashboard',
    href: routes.dashboard,
    icon: LayoutDashboard,
  },
  {
    title: 'Users',
    href: routes.adminUsers,
    icon: ShieldCheck,
  },
  {
    title: 'Applications',
    href: routes.applications,
    icon: ClipboardList,
  },
  {
    title: 'Athletes',
    href: routes.athletes,
    icon: Users,
  },
  {
    title: 'Settings',
    href: routes.settings,
    icon: Settings,
  },
];

/**
 * Get navigation items by role
 */
export function getNavigationByRole(role: UserRole): NavigationItem[] {
  switch (role) {
    case 'Admin':
      return adminNavigation;
    case 'Athlete':
      return athleteNavigation;
    case 'Coach':
    default:
      return coachNavigation;
  }
}

/** @deprecated use getNavigationByRole instead */
export const mainNavigation: NavigationItem[] = coachNavigation;

/**
 * Get navigation item by href
 */
export function getNavigationItem(href: string): NavigationItem | undefined {
  return mainNavigation.find(item => item.href === href);
}

/**
 * Check if a path is active (exact match or starts with)
 */
export function isActiveRoute(pathname: string, href: string): boolean {
  return pathname === href || pathname.startsWith(`${href}/`);
}
