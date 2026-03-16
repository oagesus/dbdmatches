import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { HistoryClient } from "./history-client";

const API_URL = process.env.API_URL || "http://localhost:5100";

export const dynamic = "force-dynamic";

const VALID_PAGE_SIZES = [20, 50, 100];
const DEFAULT_PAGE_SIZE = 20;

interface Props {
  searchParams: Promise<{
    role?: string;
    killer?: string;
    page?: string;
    pageSize?: string;
    period?: string;
  }>;
}

export default async function HistoryPage({ searchParams }: Props) {
  const cookieStore = await cookies();
  const accessToken = cookieStore.get("access_token")?.value;

  if (!accessToken) {
    redirect("/");
  }

  const params = await searchParams;
  const role = params.role || "all";
  const killer = params.killer || "";
  const period = params.period || "";
  const page = Math.max(1, Number(params.page) || 1);
  const parsedPageSize = Number(params.pageSize);
  const pageSize = VALID_PAGE_SIZES.includes(parsedPageSize) ? parsedPageSize : DEFAULT_PAGE_SIZE;

  const cookieHeader = `access_token=${accessToken}`;

  let historyUrl = `${API_URL}/api/matches/history?role=${role}&page=${page}&pageSize=${pageSize}`;
  if (killer && role === "killer") {
    historyUrl += `&killer=${encodeURIComponent(killer)}`;
  }
  if (period) {
    historyUrl += `&period=${period}`;
  }

  const [historyRes, killersRes, streaksRes] = await Promise.all([
    fetch(historyUrl, {
      headers: { Cookie: cookieHeader },
      cache: "no-store",
    }).catch(() => null),
    fetch(`${API_URL}/api/matches/killers`, {
      headers: { Cookie: cookieHeader },
      cache: "no-store",
    }).catch(() => null),
    fetch(`${API_URL}/api/matches/streaks${period ? `?period=${period}` : ""}`, {
      headers: { Cookie: cookieHeader },
      cache: "no-store",
    }).catch(() => null),
  ]);

  const historyData = historyRes?.ok
    ? await historyRes.json()
    : { matches: [], totalCount: 0, page: 1, pageSize, totalPages: 0 };

  const killersData: string[] = killersRes?.ok
    ? await killersRes.json()
    : [];

  const streaksData = streaksRes?.ok
    ? await streaksRes.json()
    : { overall: { current: 0, best: 0 }, killer: { current: 0, best: 0 }, survivor: { current: 0, best: 0 }, killers: [] };

  return (
    <HistoryClient
      initialMatches={historyData.matches}
      initialTotalCount={historyData.totalCount}
      initialTotalPages={historyData.totalPages}
      initialPlayedKillers={killersData}
      initialRole={role}
      initialKiller={killer}
      initialPeriod={period}
      initialPage={page}
      initialPageSize={pageSize}
      initialStreaks={streaksData}
    />
  );
}
