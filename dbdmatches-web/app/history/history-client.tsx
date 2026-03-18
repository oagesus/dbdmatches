"use client";

import { useEffect, useState, useRef } from "react";
import { useRouter } from "next/navigation";
import { Badge } from "@/components/ui/badge";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Clock, Loader2, Flame } from "lucide-react";
import type { MatchHistoryItem, StreakData } from "@/lib/types";
import { PaginationControls } from "@/components/pagination-controls";
import { KillerSelect } from "@/components/killer-select";
import { PeriodSelect } from "@/components/period-select";
import { MatchDetectionCard } from "@/components/match-detection-card";
import { killerRequirements } from "@/lib/killer-requirements";

interface HistoryClientProps {
  initialMatches: MatchHistoryItem[];
  initialTotalCount: number;
  initialTotalPages: number;
  initialPlayedKillers: string[];
  initialRole: string;
  initialKiller: string;
  initialPeriod: string;
  initialPage: number;
  initialPageSize: number;
  initialStreaks: StreakData;
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


function ResultBadge({ result }: { result: string }) {
  const colors: Record<string, string> = {
    Win: "bg-green-500/15 text-green-700 dark:text-green-400",
    Loss: "bg-red-500/15 text-red-700 dark:text-red-400",
    Draw: "bg-yellow-500/15 text-yellow-700 dark:text-yellow-400",
  };

  return (
    <Badge className={`${colors[result] || ""} border-transparent`}>
      {result}
    </Badge>
  );
}

function RoleLabel({ role, killerName }: { role: string; killerName?: string }) {
  return (
    <span className="text-sm font-medium">
      {role === "killer" ? killerName || "Killer" : "Survivor"}
    </span>
  );
}

function MatchCard({ match }: { match: MatchHistoryItem }) {
  const resultColors: Record<string, string> = {
    Win: "border-green-500/40 bg-green-500/5 hover:border-green-500 hover:bg-green-500/10",
    Loss: "border-red-500/40 bg-red-500/5 hover:border-red-500 hover:bg-red-500/10",
    Draw: "border-yellow-500/40 bg-yellow-500/5 hover:border-yellow-500 hover:bg-yellow-500/10",
  };

  const contaminated = match.isContaminated;
  const cardColor = contaminated
    ? "border-blue-500/40 bg-blue-500/5 hover:border-blue-500 hover:bg-blue-500/10"
    : resultColors[match.result] || "border-border bg-card";

  return (
    <div className={`group rounded-lg border p-4 ${cardColor}`}>
      <div className="flex flex-col gap-3">
        <div className="flex items-center justify-between gap-2">
          <div className="flex items-center gap-2 flex-wrap">
            <RoleLabel role={match.role} killerName={match.killer?.killerName} />
            {contaminated ? (
              <Badge variant="outline" className="bg-blue-500/20 text-blue-500 border-blue-500/30">Contaminated</Badge>
            ) : (
              <ResultBadge result={match.result} />
            )}
          </div>
          <div className="flex items-center gap-1.5 text-xs text-muted-foreground shrink-0">
            <Clock className="h-3 w-3" />
            {formatTimeAgo(match.playedAt)}
          </div>
        </div>

        {contaminated ? (
          <div className="flex flex-wrap gap-x-4 gap-y-1 text-sm text-muted-foreground">
            {match.role === "killer" && (
              <span>
                Kills: <span className="text-foreground font-medium">?</span>
                <span className="text-xs italic ml-2">Match data may be inaccurate due to non-full-loadout matches played before this match. This match does not count towards streaks.</span>
              </span>
            )}
          </div>
        ) : (
          <div className="flex flex-wrap gap-x-4 gap-y-1 text-sm text-muted-foreground">
            {match.role === "killer" && match.killer && (
              <span>
                Kills: <span className="text-foreground font-medium">{match.killer.sacrifices + match.killer.kills}</span>
                {match.killer.killerName === "Untracked Killer" && (
                  <span className="text-xs italic ml-2">No trackable killer-specific Steam achievement was detected for this match.</span>
                )}
              </span>
            )}

            {match.role === "survivor" && match.survivor && (
              <>
                <span>
                  Gate Escape: <span className="text-foreground font-medium">{match.survivor.escaped && !match.survivor.hatchEscape ? "Yes" : "No"}</span>
                </span>
                <span>
                  Hatch Escape: <span className="text-foreground font-medium">{match.survivor.hatchEscape ? "Yes" : "No"}</span>
                </span>
              </>
            )}
          </div>
        )}
      </div>
    </div>
  );
}


export function HistoryClient({
  initialMatches,
  initialTotalCount,
  initialTotalPages,
  initialPlayedKillers,
  initialRole,
  initialKiller,
  initialPeriod,
  initialPage,
  initialPageSize,
  initialStreaks,
}: HistoryClientProps) {
  const router = useRouter();
  const [streaks, setStreaks] = useState<StreakData>(initialStreaks);
  const [matches, setMatches] = useState<MatchHistoryItem[]>(initialMatches);
  const [totalPages, setTotalPages] = useState(initialTotalPages);
  const [totalCount, setTotalCount] = useState(initialTotalCount);
  const [playedKillers, setPlayedKillers] = useState<string[]>(initialPlayedKillers);
  const [refreshKey, setRefreshKey] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const currentRole = initialRole;
  const currentKiller = initialKiller;
  const currentPeriod = initialPeriod;
  const currentPage = initialPage;
  const currentPageSize = initialPageSize;

  const hasSSRData = useRef(true);

  useEffect(() => {
    const now = new Date();
    const msUntilNextMinute = (60 - now.getSeconds()) * 1000 - now.getMilliseconds();

    let interval: NodeJS.Timeout;
    const timeout = setTimeout(() => {
      setRefreshKey((k) => k + 1);
      interval = setInterval(() => {
        setRefreshKey((k) => k + 1);
      }, 60 * 1000);
    }, msUntilNextMinute);

    return () => {
      clearTimeout(timeout);
      clearInterval(interval);
    };
  }, []);

  const buildUrl = (role: string, page: number, pageSize: number, killer?: string, period?: string) => {
    const params = new URLSearchParams();
    if (role !== "all") params.set("role", role);
    if (killer) params.set("killer", killer);
    if (period) params.set("period", period);
    if (page > 1) params.set("page", page.toString());
    if (pageSize !== 20) params.set("pageSize", pageSize.toString());
    const query = params.toString();
    return query ? `/history?${query}` : "/history";
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

  useEffect(() => {
    setMatches(initialMatches);
    setTotalCount(initialTotalCount);
    setTotalPages(initialTotalPages);
    setPlayedKillers(initialPlayedKillers);
    setStreaks(initialStreaks);
  }, [initialMatches, initialTotalCount, initialTotalPages, initialPlayedKillers, initialStreaks]);

  useEffect(() => {
    if (hasSSRData.current && refreshKey === 0) {
      hasSSRData.current = false;
      return;
    }

    async function fetchHistory() {
      setError(null);

      try {
        let url = `/api/matches/history?role=${currentRole}&page=${currentPage}&pageSize=${currentPageSize}`;
        if (currentKiller && currentRole === "killer") {
          url += `&killer=${encodeURIComponent(currentKiller)}`;
        }
        if (currentPeriod) {
          url += `&period=${currentPeriod}`;
        }
        const res = await fetch(url);
        if (!res.ok) {
          setError("Failed to load match history");
          return;
        }
        const data = await res.json();
        setMatches(data.matches);
        setTotalPages(data.totalPages);
        setTotalCount(data.totalCount);
      } catch {
        setError("Failed to load match history");
      } finally {
        setLoading(false);
      }
    }

    fetchHistory();
  }, [currentRole, currentKiller, currentPage, currentPageSize, refreshKey]);

  // Fetch killers list client-side on refresh (in case new killers were played)
  useEffect(() => {
    if (refreshKey === 0) return;
    async function fetchKillers() {
      try {
        const res = await fetch("/api/matches/killers");
        if (res.ok) {
          const data: string[] = await res.json();
          setPlayedKillers(data);
        }
      } catch { /* ignore */ }
    }
    fetchKillers();
  }, [refreshKey]);

  useEffect(() => {
    if (refreshKey === 0) return;
    async function fetchStreaks() {
      try {
        const res = await fetch(`/api/matches/streaks${currentPeriod ? `?period=${currentPeriod}` : ""}`);
        if (res.ok) {
          const data: StreakData = await res.json();
          setStreaks(data);
        }
      } catch { /* ignore */ }
    }
    fetchStreaks();
  }, [refreshKey]);

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

  return (
    <div className="flex flex-1 flex-col gap-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Match History</h1>
        <p className="text-sm text-muted-foreground">{totalCount} matches played</p>
      </div>

      <Tabs value={currentRole} onValueChange={setRole}>
        <TabsList>
          <TabsTrigger value="all" className="cursor-pointer data-[state=active]:bg-primary data-[state=active]:text-primary-foreground dark:data-[state=active]:bg-primary dark:data-[state=active]:text-primary-foreground dark:data-[state=active]:border-primary">All</TabsTrigger>
          <TabsTrigger value="killer" className="cursor-pointer data-[state=active]:bg-primary data-[state=active]:text-primary-foreground dark:data-[state=active]:bg-primary dark:data-[state=active]:text-primary-foreground dark:data-[state=active]:border-primary">Killer</TabsTrigger>
          <TabsTrigger value="survivor" className="cursor-pointer data-[state=active]:bg-primary data-[state=active]:text-primary-foreground dark:data-[state=active]:bg-primary dark:data-[state=active]:text-primary-foreground dark:data-[state=active]:border-primary">Survivor</TabsTrigger>
        </TabsList>

        <div className="flex flex-col gap-2 pt-2 sm:flex-row sm:flex-wrap sm:items-center sm:justify-between">
          <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:gap-3">
            {currentRole === "killer" && playedKillers.length > 0 && (
              <KillerSelect value={currentKiller} killers={playedKillers} onChange={setKiller} />
            )}
            <PeriodSelect value={currentPeriod} onChange={setPeriod} />
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

        <MatchDetectionCard role={currentRole} killer={currentKiller} />

        <div className="flex flex-col gap-1 sm:flex-row sm:items-center sm:gap-4 pt-4">
          <div className="flex items-center gap-2">
            <span className="text-foreground font-semibold text-base">Current {streakLabel} Streak:</span>
            <Flame className="h-6 w-6 text-orange-500" />
            <span className="font-bold text-2xl leading-none text-orange-500">{activeStreaks.current}</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-foreground font-semibold text-base">Best {streakLabel} Streak:</span>
            <Flame className="h-6 w-6 text-orange-500" />
            <span className="font-bold text-2xl leading-none text-orange-500">{activeStreaks.best}</span>
          </div>
        </div>

        <TabsContent value={currentRole}>
          {loading && (
            <div className="flex items-center justify-center py-16">
              <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            </div>
          )}

          {error && (
            <div className="flex items-center justify-center py-16">
              <p className="text-sm text-destructive">{error}</p>
            </div>
          )}

          {!loading && !error && matches.length === 0 && (
            <div className="flex flex-col items-center justify-center py-16 text-center">
              {currentKiller && killerRequirements[currentKiller] === "not trackable" ? (
                <>
                  <p className="text-destructive">{currentKiller} matches cannot be tracked.</p>
                  <p className="mt-1 text-sm text-destructive">
                    This killer does not have a unique Steam achievement to track.
                  </p>
                </>
              ) : (
                <>
                  <p className="text-muted-foreground">No matches found.</p>
                  <p className="mt-1 text-sm text-muted-foreground">
                    Start playing{currentKiller ? ` ${currentKiller}` : currentRole === "survivor" ? " Survivor" : currentRole === "killer" ? " Killer" : ""} and your matches will appear here.
                  </p>
                </>
              )}
            </div>
          )}

          {!loading && !error && matches.length > 0 && (
            <>
              <div className="grid gap-3">
                {matches.map((match) => (
                  <MatchCard key={match.publicId} match={match} />
                ))}
              </div>

              <PaginationControls currentPage={currentPage} totalPages={totalPages} onPageChange={setPage} />
            </>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
