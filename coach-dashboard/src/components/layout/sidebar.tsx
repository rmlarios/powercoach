'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { 
  Dumbbell, 
  ChevronLeft,
  ChevronRight,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { useState } from 'react';
import { getNavigationByRole, isActiveRoute } from '@/config';
import { useAuth } from '@/providers/auth-provider';

export function Sidebar() {
  const pathname = usePathname();
  const [collapsed, setCollapsed] = useState(false);
  const { user } = useAuth();
  const role = user?.role ?? 'Coach';
  const navigation = getNavigationByRole(role);

  return (
    <aside 
      className={cn(
        "relative flex flex-col h-screen bg-slate-900 text-white transition-all duration-300",
        collapsed ? "w-16" : "w-64"
      )}
    >
      {/* Logo */}
      <div className={cn(
        "flex items-center h-16 px-4 border-b border-slate-800",
        collapsed ? "justify-center" : "gap-3"
      )}>
        <div className="flex items-center justify-center w-8 h-8 bg-blue-600 rounded-lg">
          <Dumbbell className="w-5 h-5" />
        </div>
        {!collapsed && (
          <span className="text-lg font-semibold">CoachPlatform</span>
        )}
      </div>

      {/* Navigation */}
      <nav className="flex-1 p-2 space-y-1 overflow-y-auto">
        {navigation.map((item) => {
          const isActive = isActiveRoute(pathname, item.href);
          
          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors",
                isActive 
                  ? "bg-blue-600 text-white" 
                  : "text-slate-300 hover:bg-slate-800 hover:text-white",
                collapsed && "justify-center px-0"
              )}
              title={collapsed ? item.title : undefined}
            >
              <item.icon className="w-5 h-5 flex-shrink-0" />
              {!collapsed && <span>{item.title}</span>}
            </Link>
          );
        })}
      </nav>

      {/* Collapse Button */}
      <Button
        variant="ghost"
        size="icon"
        className="absolute -right-3 top-20 w-6 h-6 bg-slate-700 hover:bg-slate-600 rounded-full border border-slate-600"
        onClick={() => setCollapsed(!collapsed)}
      >
        {collapsed ? (
          <ChevronRight className="w-4 h-4" />
        ) : (
          <ChevronLeft className="w-4 h-4" />
        )}
      </Button>

      {/* Footer */}
      <div className={cn(
        "p-4 border-t border-slate-800 text-xs text-slate-500",
        collapsed && "text-center p-2"
      )}>
        {collapsed ? "v1" : "Version 1.0.0"}
      </div>
    </aside>
  );
}
