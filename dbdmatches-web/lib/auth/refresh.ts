import { NextRequest, NextResponse } from "next/server";

const API_URL = process.env.API_URL || "http://localhost:5100";

function isTokenExpired(request: NextRequest): boolean {
  const accessToken = request.cookies.get("access_token")?.value;
  if (!accessToken) return true;

  try {
    const payload = JSON.parse(atob(accessToken.split(".")[1]));
    return payload.exp * 1000 < Date.now();
  } catch {
    return true;
  }
}

export async function handleTokenRefresh(
  request: NextRequest
): Promise<{ response: NextResponse | null; isAuthenticated: boolean }> {
  const refreshToken = request.cookies.get("refresh_token")?.value;

  if (!refreshToken) {
    return { response: null, isAuthenticated: false };
  }

  if (!isTokenExpired(request)) {
    return { response: null, isAuthenticated: true };
  }

  try {
    const res = await fetch(`${API_URL}/api/auth/refresh`, {
      method: "POST",
      headers: {
        Cookie: `refresh_token=${refreshToken}`,
      },
    });

    if (!res.ok) {
      return { response: null, isAuthenticated: false };
    }

    const response = NextResponse.next();

    const setCookieHeaders = res.headers.getSetCookie();
    for (const cookie of setCookieHeaders) {
      const [nameValue, ...parts] = cookie.split(";");
      const [name, value] = nameValue.split("=");
      const isHttpOnly = parts.some((p) => p.trim().toLowerCase() === "httponly");

      response.cookies.set({
        name: name.trim(),
        value: value.trim(),
        httpOnly: isHttpOnly,
        sameSite: "lax",
        path: "/",
        secure: process.env.NODE_ENV === "production",
      });
    }

    return { response, isAuthenticated: true };
  } catch {
    return { response: null, isAuthenticated: false };
  }
}
