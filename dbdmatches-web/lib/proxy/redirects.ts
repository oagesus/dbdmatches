import { NextRequest, NextResponse } from "next/server";

const PROTECTED_ROUTES = ["/dashboard", "/account"];

export function handleAuthRedirects(
  request: NextRequest,
  isAuthenticated: boolean
): NextResponse | null {
  const { pathname } = request.nextUrl;


  if (pathname === "/auth/clear") return null;


  if (!isAuthenticated && PROTECTED_ROUTES.some((route) => pathname.startsWith(route))) {
    return NextResponse.redirect(new URL("/", request.url));
  }

  return null;
}
