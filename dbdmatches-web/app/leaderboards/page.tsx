import { getLeaderboard } from "@/lib/leaderboard/get-leaderboard";
import { LeaderboardClient } from "./leaderboard-client";

export const dynamic = "force-dynamic";

const VALID_PAGE_SIZES = [20, 50, 100];
const DEFAULT_PAGE_SIZE = 20;

const API_URL = process.env.API_URL || "http://localhost:5100";

interface Props {
  searchParams: Promise<{
    role?: string;
    killer?: string;
    period?: string;
    page?: string;
    pageSize?: string;
  }>;
}

export default async function LeaderboardPage({ searchParams }: Props) {
  const params = await searchParams;
  const role = params.role || "all";
  const killer = params.killer || "";
  const period = params.period || "";
  const page = Math.max(1, Number(params.page) || 1);
  const parsedPageSize = Number(params.pageSize);
  const pageSize = VALID_PAGE_SIZES.includes(parsedPageSize) ? parsedPageSize : DEFAULT_PAGE_SIZE;

  const [leaderboardData, killersRes] = await Promise.all([
    getLeaderboard(role, killer, period, page, pageSize),
    fetch(`${API_URL}/api/matches/killers`, { cache: "no-store" }).catch(() => null),
  ]);

  const killersData: string[] = killersRes?.ok ? await killersRes.json() : [];

  const now = new Date();
  const minutesUntilNextHour = 60 - now.getMinutes();

  return (
    <LeaderboardClient
      initialItems={leaderboardData.items}
      initialTotalPages={leaderboardData.totalPages}
      initialKillers={killersData}
      initialRole={role}
      initialKiller={killer}
      initialPeriod={period}
      initialPage={page}
      initialPageSize={pageSize}
      initialMinutesUntilRefresh={minutesUntilNextHour}
    />
  );
}
