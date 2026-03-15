import { cookies } from "next/headers";

const API_URL = process.env.API_URL || "http://localhost:5100";

export interface User {
  publicId: string;
  steamId: string;
  displayName: string;
  avatarUrl: string | null;
  createdAt: string;
}

export async function getUser(): Promise<User | null> {
  const cookieStore = await cookies();
  const accessToken = cookieStore.get("access_token")?.value;

  if (!accessToken) return null;

  try {
    const res = await fetch(`${API_URL}/api/auth/me`, {
      headers: {
        Cookie: `access_token=${accessToken}`,
      },
      cache: "no-store",
    });

    if (!res.ok) return null;

    return await res.json();
  } catch {
    return null;
  }
}
