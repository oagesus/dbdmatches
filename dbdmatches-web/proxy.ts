import { NextRequest, NextResponse } from "next/server";
import { handleTokenRefresh } from "@/lib/auth/refresh";
import { handleAuthRedirects } from "@/lib/proxy/redirects";

export async function proxy(request: NextRequest) {
  const { response: refreshResponse, isAuthenticated } =
    await handleTokenRefresh(request);

  const redirectResponse = handleAuthRedirects(request, isAuthenticated);
  if (redirectResponse) return redirectResponse;

  return refreshResponse ?? NextResponse.next();
}

export const config = {
  matcher: [
    "/((?!api/|_next/static|_next/image|favicon.ico|.*\\.(?:svg|png|jpg|jpeg|gif|webp)$).*)",
  ],
};
