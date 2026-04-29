import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

const PUBLIC_PATHS = ['/login', '/apply'];

// Routes only accessible by Coach / Admin
const COACH_ONLY_PATHS = ['/dashboard', '/athletes', '/programs', '/exercises', '/check-ins', '/applications', '/plans', '/subscriptions', '/payments'];
// Routes only accessible by Athlete (exact path or sub-paths like /athlete/check-in)
const ATHLETE_ONLY_PATHS = ['/athlete/'];

export function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // Allow public paths through
  if (PUBLIC_PATHS.some((p) => pathname.startsWith(p))) {
    return NextResponse.next();
  }

  // Allow Next.js internals and static files
  if (
    pathname.startsWith('/_next') ||
    pathname.startsWith('/api') ||
    pathname.includes('.')
  ) {
    return NextResponse.next();
  }

  // Check for access token cookie (set by login)
  const token = request.cookies.get('cp_access_token')?.value;

  if (!token) {
    const loginUrl = new URL('/login', request.url);
    loginUrl.searchParams.set('from', pathname);
    return NextResponse.redirect(loginUrl);
  }

  // Role-based routing
  const role = request.cookies.get('cp_user_role')?.value;

  if (role === 'Athlete') {
    // Athletes trying to access coach routes → redirect to athlete dashboard
    if (COACH_ONLY_PATHS.some((p) => pathname.startsWith(p))) {
      return NextResponse.redirect(new URL('/athlete', request.url));
    }
  } else if (role === 'Coach' || role === 'Admin') {
    // Coaches/Admins trying to access athlete portal → redirect to coach dashboard
    if (pathname === '/athlete' || ATHLETE_ONLY_PATHS.some((p) => pathname.startsWith(p))) {
      return NextResponse.redirect(new URL('/dashboard', request.url));
    }
  }

  // Root redirect based on role
  if (pathname === '/') {
    const home = role === 'Athlete' ? '/athlete' : '/dashboard';
    return NextResponse.redirect(new URL(home, request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/((?!_next/static|_next/image|favicon.ico).*)'],
};
