"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";

import { Badge } from "@/components/ui/badge";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Clock, ArrowLeft, Flame } from "lucide-react";
import type { MatchHistoryItem, StreakData } from "@/lib/types";
import { PaginationControls } from "@/components/pagination-controls";
import { statusColor } from "@/lib/status-utils";

interface PlayerInfo {
  steamId: string;
  displayName: string;
  vanityUrl: string | null;
  avatarUrl: string | null;
  status: string;
}

interface PlayerMatchesClientProps {
  steamId: string;
  player: PlayerInfo;
  initialMatches: MatchHistoryItem[];
  initialTotalCount: number;
  initialTotalPages: number;
  initialKillers: string[];
  initialRole: string;
  initialKiller: string;
  initialPeriod: string;
  initialPage: number;
  initialPageSize: number;
  initialStreaks: StreakData;
}


const resultColors: Record<string, string> = {
  Win: "border-green-500/40 bg-green-500/5 hover:border-green-500 hover:bg-green-500/10",
  Loss: "border-red-500/40 bg-red-500/5 hover:border-red-500 hover:bg-red-500/10",
  Draw: "border-yellow-500/40 bg-yellow-500/5 hover:border-yellow-500 hover:bg-yellow-500/10",
};

function ResultBadge({ result }: { result: string }) {
  const colors: Record<string, string> = {
    Win: "bg-green-500/20 text-green-500 border-green-500/30",
    Loss: "bg-red-500/20 text-red-500 border-red-500/30",
    Draw: "bg-yellow-500/20 text-yellow-500 border-yellow-500/30",
  };
  return <Badge variant="outline" className={colors[result] || ""}>{result}</Badge>;
}

function formatBP(bp: number): string {
  return bp.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function formatTimeAgo(dateString: string): string {
  const date = new Date(dateString);
  const now = new Date();
  const seconds = Math.floor((now.getTime() - date.getTime()) / 1000);

  if (seconds < 60) return "just now";
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;

  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

function MatchCard({ match }: { match: MatchHistoryItem }) {
  return (
    <div className={`rounded-lg border p-4 ${resultColors[match.result] || "border-border bg-card"}`}>
      <div className="flex flex-col gap-3">
        <div className="flex items-center justify-between gap-2">
          <div className="flex items-center gap-2 flex-wrap">
            <span className="font-medium">
              {match.role === "killer" ? match.killer?.killerName : "Survivor"}
            </span>
            <ResultBadge result={match.result} />
          </div>
          <div className="flex items-center gap-1.5 text-xs text-muted-foreground shrink-0">
            <Clock className="h-3.5 w-3.5" />
            {formatTimeAgo(match.playedAt)}
          </div>
        </div>
        <div className="flex flex-wrap gap-x-4 gap-y-1 text-sm text-muted-foreground">
          {match.killer && (
            <>
              <span>Kills: <span className="text-foreground font-medium">{match.killer.sacrifices + match.killer.kills}</span></span>
              {match.killer.killerName === "Untracked Killer" ? (
                <span className="italic">No trackable killer power stats found</span>
              ) : (
                <>
                  {match.killer.powerStat1 > 0 && (
                    <span>{match.killer.powerStat1Label}: <span className="text-foreground font-medium">{match.killer.powerStat1}</span></span>
                  )}
                  {match.killer.powerStat2Label && match.killer.powerStat2 > 0 && (
                    <span>{match.killer.powerStat2Label}: <span className="text-foreground font-medium">{match.killer.powerStat2}</span></span>
                  )}
                  {match.killer.powerStat3Label && match.killer.powerStat3 > 0 && (
                    <span>{match.killer.powerStat3Label}: <span className="text-foreground font-medium">{match.killer.powerStat3}</span></span>
                  )}
                </>
              )}
            </>
          )}
          {match.survivor && (
            <>
              <span>Gate Escape: <span className="text-foreground font-medium">{match.survivor.escaped && !match.survivor.hatchEscape ? "Yes" : "No"}</span></span>
              <span>Hatch Escape: <span className="text-foreground font-medium">{match.survivor.hatchEscape ? "Yes" : "No"}</span></span>
              <span>Generators: <span className="text-foreground font-medium">{match.survivor.generatorsCompleted}</span></span>
            </>
          )}
          <span>
            BP: <span className="text-foreground font-medium">{formatBP(match.bloodpointsEarned)}</span>
          </span>
        </div>
      </div>
    </div>
  );
}

export function PlayerMatchesClient({
  steamId,
  player,
  initialMatches,
  initialTotalCount,
  initialTotalPages,
  initialKillers,
  initialRole,
  initialKiller,
  initialPeriod,
  initialPage,
  initialPageSize,
  initialStreaks,
}: PlayerMatchesClientProps) {
  const router = useRouter();
  const [matches, setMatches] = useState(initialMatches);
  const [totalPages, setTotalPages] = useState(initialTotalPages);
  const [totalCount, setTotalCount] = useState(initialTotalCount);
  const [killers, setKillers] = useState(initialKillers);
  const [streaks, setStreaks] = useState<StreakData>(initialStreaks);

  useEffect(() => {
    setMatches(initialMatches);
    setTotalPages(initialTotalPages);
    setTotalCount(initialTotalCount);
    setKillers(initialKillers);
    setStreaks(initialStreaks);
  }, [initialMatches, initialTotalPages, initialTotalCount, initialKillers, initialStreaks]);

  useEffect(() => {
    const now = new Date();
    const msUntilNextMinute = (60 - now.getSeconds()) * 1000 - now.getMilliseconds();

    const timeout = setTimeout(() => {
      router.refresh();
      const interval = setInterval(() => {
        router.refresh();
      }, 60000);
      return () => clearInterval(interval);
    }, msUntilNextMinute);

    return () => clearTimeout(timeout);
  }, [router]);

  const currentRole = initialRole;
  const currentKiller = initialKiller;
  const currentPeriod = initialPeriod;
  const currentPage = initialPage;
  const currentPageSize = initialPageSize;

  const activeStreaks = currentRole === "killer"
    ? currentKiller
      ? streaks.killers.find(k => k.killer === currentKiller) ?? { current: 0, best: 0 }
      : streaks.killer
    : currentRole === "survivor"
      ? streaks.survivor
      : streaks.overall;

  const streakLabel = currentRole === "killer"
    ? currentKiller
      ? currentKiller
      : "All Killer"
    : currentRole === "survivor"
      ? "Survivor"
      : "Overall";

  const buildUrl = (role: string, page: number, pageSize: number, killer?: string, period?: string) => {
    const params = new URLSearchParams();
    if (role !== "all") params.set("role", role);
    if (killer) params.set("killer", killer);
    if (period) params.set("period", period);
    if (page > 1) params.set("page", page.toString());
    if (pageSize !== 20) params.set("pageSize", pageSize.toString());
    const query = params.toString();
    return query ? `/leaderboards/${steamId}?${query}` : `/leaderboards/${steamId}`;
  };

  const setRole = (newRole: string) => {
    router.push(buildUrl(newRole, 1, currentPageSize, undefined, currentPeriod));
  };

  const setPage = (newPage: number) => {
    router.push(buildUrl(currentRole, newPage, currentPageSize, currentKiller, currentPeriod));
  };

  const setPageSize = (newSize: number) => {
    router.push(buildUrl(currentRole, 1, newSize, currentKiller, currentPeriod));
  };

  const setKiller = (newKiller: string) => {
    router.push(buildUrl(currentRole, 1, currentPageSize, newKiller || undefined, currentPeriod));
  };

  const setPeriod = (newPeriod: string) => {
    router.push(buildUrl(currentRole, 1, currentPageSize, currentKiller, newPeriod === "all" ? undefined : newPeriod));
  };

  return (
    <div className="flex flex-1 flex-col gap-6">
      <div className="flex flex-col gap-4">
        <button
          onClick={() => {
            if (typeof window !== "undefined") {
              const returnUrl = sessionStorage.getItem("leaderboard_return_url");
              if (returnUrl) {
                sessionStorage.removeItem("leaderboard_return_url");
                router.push(returnUrl);
              } else {
                router.push("/leaderboards");
              }
            }
          }}
          className="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground w-fit cursor-pointer"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Leaderboards
        </button>
        <div className="flex items-center gap-4">
          <div className="relative inline-block">
            {player.avatarUrl ? (
              <img src={player.avatarUrl} alt={player.displayName} className="h-14 w-14 rounded-full" />
            ) : (
              <div className="h-14 w-14 rounded-full bg-muted" />
            )}
            <span className={`absolute bottom-0 right-0 h-4 w-4 rounded-full border-2 border-background ${statusColor(player.status as "Offline" | "Online" | "InGame")}`} />
          </div>
          <div>
            <h1 className="text-2xl font-bold">{player.displayName}</h1>
            <a href={player.vanityUrl ? `https://steamcommunity.com/id/${player.vanityUrl}/` : `https://steamcommunity.com/profiles/${steamId}/`} target="_blank" rel="noopener noreferrer" className="text-sm text-muted-foreground hover:text-foreground">{player.vanityUrl ? `steamcommunity.com/id/${player.vanityUrl}/` : `steamcommunity.com/profiles/${steamId}/`}</a>
            <p className="text-sm text-muted-foreground">{totalCount} matches played</p>
          </div>
        </div>
      </div>

      <Tabs value={currentRole} onValueChange={setRole}>
        <TabsList>
          <TabsTrigger value="all" className="cursor-pointer data-[state=active]:bg-primary data-[state=active]:text-primary-foreground dark:data-[state=active]:bg-primary dark:data-[state=active]:text-primary-foreground">All</TabsTrigger>
          <TabsTrigger value="killer" className="cursor-pointer data-[state=active]:bg-primary data-[state=active]:text-primary-foreground dark:data-[state=active]:bg-primary dark:data-[state=active]:text-primary-foreground">Killer</TabsTrigger>
          <TabsTrigger value="survivor" className="cursor-pointer data-[state=active]:bg-primary data-[state=active]:text-primary-foreground dark:data-[state=active]:bg-primary dark:data-[state=active]:text-primary-foreground">Survivor</TabsTrigger>
        </TabsList>

        <div className="flex flex-col gap-2 pt-2 sm:flex-row sm:flex-wrap sm:items-center sm:justify-between">
          <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:gap-3">
            {currentRole === "killer" && killers.length > 0 && (
              <Select value={currentKiller || "all"} onValueChange={(v) => setKiller(v === "all" ? "" : v)}>
                <SelectTrigger className="h-7 w-[180px] text-xs cursor-pointer">
                  <SelectValue placeholder="All Killers" />
                </SelectTrigger>
                <SelectContent position="popper" side="bottom" align="start">
                  <SelectItem value="all" className="cursor-pointer text-xs">All Killers</SelectItem>
                  {killers.map((k) => (
                    <SelectItem key={k} value={k} className="cursor-pointer text-xs">{k}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            )}
            <Select value={currentPeriod || "all"} onValueChange={setPeriod}>
              <SelectTrigger className="h-7 w-[130px] text-xs cursor-pointer">
                <SelectValue>{{ "all": "All Time", "30d": "Last 30 Days", "90d": "Last 90 Days", "1y": "Last Year" }[currentPeriod || "all"]}</SelectValue>
              </SelectTrigger>
              <SelectContent position="popper" side="bottom" align="start">
                <SelectItem value="all" className="cursor-pointer text-xs">All Time</SelectItem>
                <SelectItem value="30d" className="cursor-pointer text-xs">Last 30 Days</SelectItem>
                <SelectItem value="90d" className="cursor-pointer text-xs">Last 90 Days</SelectItem>
                <SelectItem value="1y" className="cursor-pointer text-xs">Last Year</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <Select value={currentPageSize.toString()} onValueChange={(v) => setPageSize(Number(v))}>
            <SelectTrigger className="h-7 w-[100px] text-xs cursor-pointer">
              <SelectValue>{currentPageSize} / page</SelectValue>
            </SelectTrigger>
            <SelectContent position="popper" side="bottom" align="end">
              <SelectItem value="20" className="cursor-pointer text-xs">20 / page</SelectItem>
              <SelectItem value="50" className="cursor-pointer text-xs">50 / page</SelectItem>
              <SelectItem value="100" className="cursor-pointer text-xs">100 / page</SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div className="flex flex-col gap-1 sm:flex-row sm:items-center sm:gap-4 text-sm">
          <div className="flex items-center gap-2">
            <span className="text-muted-foreground">Current {streakLabel} Streak:</span>
            <Flame className="h-4 w-4 text-primary" />
            <span className="font-bold text-lg">{activeStreaks.current}</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-muted-foreground">Best {streakLabel} Streak:</span>
            <Flame className="h-4 w-4 text-primary" />
            <span className="font-bold text-lg">{activeStreaks.best}</span>
          </div>
        </div>

        <div className="grid gap-3 pt-4">
          {matches.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-16 text-muted-foreground">
              <p>No matches found</p>
            </div>
          ) : (
            matches.map((match) => (
              <MatchCard key={match.publicId} match={match} />
            ))
          )}
        </div>
      </Tabs>

      <PaginationControls currentPage={currentPage} totalPages={totalPages} onPageChange={setPage} />
    </div>
  );
}
