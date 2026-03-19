import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { PlayerMatchesClient } from "./player-matches-client";

export const dynamic = "force-dynamic";

const API_URL = process.env.API_URL || "http://localhost:5100";
const VALID_PAGE_SIZES = [20, 50, 100];
const DEFAULT_PAGE_SIZE = 20;

interface Props {
  params: Promise<{ steamId: string }>;
  searchParams: Promise<{
    role?: string;
    killer?: string;
    period?: string;
    page?: string;
    pageSize?: string;
  }>;
}

export default async function PlayerPage({ params, searchParams }: Props) {
  const { steamId } = await params;
  const sp = await searchParams;
  const role = sp.role || "all";
  const killer = sp.killer || "";
  const period = sp.period || "";
  const page = Math.max(1, Number(sp.page) || 1);
  const parsedPageSize = Number(sp.pageSize);
  const pageSize = VALID_PAGE_SIZES.includes(parsedPageSize) ? parsedPageSize : DEFAULT_PAGE_SIZE;

  let url = `${API_URL}/api/leaderboard/${steamId}/matches?role=${role}&page=${page}&pageSize=${pageSize}`;
  if (killer && role === "killer") url += `&killer=${encodeURIComponent(killer)}`;
  if (period) url += `&period=${period}`;

  const [matchesRes, killersRes] = await Promise.all([
    fetch(url, { cache: "no-store" }).catch(() => null),
    fetch(`${API_URL}/api/matches/killers`, { cache: "no-store" }).catch(() => null),
  ]);

  if (!matchesRes?.ok) {
    return (
      <div className="flex flex-1 flex-col gap-6">
        <Link href="/leaderboards" className="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground w-fit cursor-pointer">
          <ArrowLeft className="h-4 w-4" />
          Back to Leaderboards
        </Link>
        <div className="flex flex-col items-center justify-center py-16">
          <p className="text-sm text-muted-foreground">Player not found.</p>
          <p className="text-sm text-muted-foreground">This player has not signed in to DBDMatches yet.</p>
        </div>
      </div>
    );
  }

  const data = await matchesRes.json();
  const killersData: string[] = killersRes?.ok ? await killersRes.json() : [];

  return (
    <PlayerMatchesClient
      steamId={steamId}
      player={data.player}
      initialMatches={data.matches}
      initialTotalCount={data.totalCount}
      initialTotalPages={data.totalPages}
      initialKillers={killersData}
      initialRole={role}
      initialKiller={killer}
      initialPeriod={period}
      initialPage={page}
      initialPageSize={pageSize}
      initialStreaks={data.streaks}
    />
  );
}
