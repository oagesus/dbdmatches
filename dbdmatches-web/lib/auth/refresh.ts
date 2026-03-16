import { NextRequest, NextResponse } from "next/server";

const API_URL = process.env.API_URL || "http://localhost:5100";

function isTokenExpired(request: NextRequest): boolean {
  const accessToken = request.cookies.get("access_token")?.value;
  if (!accessToken) return true;

  try {
    const payload = JSON.parse(atob(accessToken.split(".")[1]));
    return Date.now() >= payload.exp * 1000 - 10000;
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
      const response = NextResponse.next();
      response.cookies.delete("access_token");
      response.cookies.delete("refresh_token");
      response.cookies.delete("token_exp");
      return { response, isAuthenticated: false };
    }

    const response = NextResponse.next();

    const setCookieHeaders = res.headers.getSetCookie();
    for (const cookie of setCookieHeaders) {
      const [cookiePart] = cookie.split(";");
      const [name, value] = cookiePart.split("=");

      if (name === "access_token" || name === "refresh_token") {
        response.cookies.set({
          name,
          value: decodeURIComponent(value),
          httpOnly: true,
          sameSite: "lax",
          path: "/",
          maxAge: name === "access_token" ? 300 : 2592000,
        });
      }

      if (name === "token_exp") {
        response.cookies.set({
          name,
          value: decodeURIComponent(value),
          httpOnly: false,
          sameSite: "lax",
          path: "/",
          maxAge: 2592000,
        });
      }
    }

    return { response, isAuthenticated: true };
  } catch {
    return { response: null, isAuthenticated: false };
  }
}
