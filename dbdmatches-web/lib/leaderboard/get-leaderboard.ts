const API_URL = process.env.API_URL || "http://localhost:5100";

export interface LeaderboardItem {
  rank: number;
  steamId: string;
  displayName: string;
  vanityUrl: string | null;
  avatarUrl: string | null;
  bestStreak: number;
  totalMatches: number;
}

export interface LeaderboardResponse {
  items: LeaderboardItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  calculatedAt: string;
}

export async function getLeaderboard(
  role: string,
  killer: string,
  period: string,
  page: number,
  pageSize: number,
  search?: string
): Promise<LeaderboardResponse> {
  let url = `${API_URL}/api/leaderboard?role=${role}&page=${page}&pageSize=${pageSize}`;
  if (killer && role === "killer") url += `&killer=${encodeURIComponent(killer)}`;
  if (period) url += `&period=${period}`;
  if (search) url += `&search=${encodeURIComponent(search)}`;

  try {
    const res = await fetch(url, { cache: "no-store" });
    if (!res.ok) throw new Error("Failed to fetch leaderboard");
    return await res.json();
  } catch {
    return {
      items: [],
      totalCount: 0,
      page: 1,
      pageSize,
      totalPages: 0,
      calculatedAt: new Date().toISOString(),
    };
  }
}
