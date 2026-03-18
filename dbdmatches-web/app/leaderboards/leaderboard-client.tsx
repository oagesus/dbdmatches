"use client";

import { useEffect, useState, useRef, useCallback } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Clock, Flame, Search } from "lucide-react";
import { KillerSelect } from "@/components/killer-select";
import { PeriodSelect } from "@/components/period-select";
import { MatchDetectionCard } from "@/components/match-detection-card";
import { killerRequirements } from "@/lib/killer-requirements";
import type { LeaderboardItem } from "@/lib/leaderboard/get-leaderboard";
import { PaginationControls } from "@/components/pagination-controls";

interface LeaderboardClientProps {
  initialItems: LeaderboardItem[];
  initialTotalPages: number;
  initialKillers: string[];
  initialRole: string;
  initialKiller: string;
  initialPeriod: string;
  initialPage: number;
  initialPageSize: number;
  initialMinutesUntilRefresh: number;
  initialSearch: string;
}


function getRankStyle(rank: number): string {
  switch (rank) {
    case 1: return "text-yellow-500 font-bold";
    case 2: return "text-gray-400 font-bold";
    case 3: return "text-amber-600 font-bold";
    default: return "text-muted-foreground";
  }
}

export function LeaderboardClient({
  initialItems,
  initialTotalPages,
  initialKillers,
  initialRole,
  initialKiller,
  initialPeriod,
  initialPage,
  initialPageSize,
  initialMinutesUntilRefresh,
  initialSearch,
}: LeaderboardClientProps) {
  const router = useRouter();
  const [items, setItems] = useState(initialItems);
  const [totalPages, setTotalPages] = useState(initialTotalPages);
  const [killers, setKillers] = useState(initialKillers);
  const [minutesLeft, setMinutesLeft] = useState(initialMinutesUntilRefresh);
  const [searchInput, setSearchInput] = useState(initialSearch);

  const currentRole = initialRole;
  const currentKiller = initialKiller;
  const currentPeriod = initialPeriod;
  const currentPage = initialPage;
  const currentPageSize = initialPageSize;
  const currentSearch = initialSearch;

  // Countdown timer - update every minute on the minute
  useEffect(() => {
    const now = new Date();
    const msUntilNextMinute = (60 - now.getSeconds()) * 1000 - now.getMilliseconds();

    let interval: NodeJS.Timeout;
    const timeout = setTimeout(() => {
      setMinutesLeft((m) => Math.max(0, m - 1));
      interval = setInterval(() => {
        setMinutesLeft((m) => Math.max(0, m - 1));
      }, 60 * 1000);
    }, msUntilNextMinute);

    return () => {
      clearTimeout(timeout);
      clearInterval(interval);
    };
  }, []);

  // Refresh data on the hour
  useEffect(() => {
    if (minutesLeft > 0) return;

    async function refresh() {
      try {
        let url = `/api/leaderboard?role=${currentRole}&page=${currentPage}&pageSize=${currentPageSize}`;
        if (currentKiller && currentRole === "killer") url += `&killer=${encodeURIComponent(currentKiller)}`;
        if (currentPeriod) url += `&period=${currentPeriod}`;

        const res = await fetch(url);
        if (res.ok) {
          const data = await res.json();
          setItems(data.items);
          setTotalPages(data.totalPages);
        }
      } catch { /* ignore */ }
      setMinutesLeft(60);
    }

    refresh();
  }, [minutesLeft, currentRole, currentKiller, currentPeriod, currentPage, currentPageSize]);

  // Sync SSR data
  useEffect(() => {
    setItems(initialItems);
    setTotalPages(initialTotalPages);
    setKillers(initialKillers);
  }, [initialItems, initialTotalPages, initialKillers]);

  const buildUrl = (role: string, page: number, pageSize: number, killer?: string, period?: string, search?: string) => {
    const params = new URLSearchParams();
    if (role !== "all") params.set("role", role);
    if (killer) params.set("killer", killer);
    if (period) params.set("period", period);
    if (search) params.set("search", search);
    if (page > 1) params.set("page", page.toString());
    if (pageSize !== 20) params.set("pageSize", pageSize.toString());
    const query = params.toString();
    return query ? `/leaderboards?${query}` : "/leaderboards";
  };

  const setRole = (newRole: string) => {
    router.push(buildUrl(newRole, 1, currentPageSize, undefined, currentPeriod, currentSearch));
  };

  const setPage = (newPage: number) => {
    router.push(buildUrl(currentRole, newPage, currentPageSize, currentKiller, currentPeriod, currentSearch));
  };

  const setPageSize = (newSize: number) => {
    router.push(buildUrl(currentRole, 1, newSize, currentKiller, currentPeriod, currentSearch));
  };

  const setKiller = (newKiller: string) => {
    router.push(buildUrl(currentRole, 1, currentPageSize, newKiller || undefined, currentPeriod, currentSearch));
  };

  const setPeriod = (newPeriod: string) => {
    router.push(buildUrl(currentRole, 1, currentPageSize, currentKiller, newPeriod === "all" ? undefined : newPeriod, currentSearch));
  };

  const debounceRef = useRef<NodeJS.Timeout | null>(null);
  const handleSearchChange = useCallback((value: string) => {
    setSearchInput(value);
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => {
      router.push(buildUrl(currentRole, 1, currentPageSize, currentKiller, currentPeriod, value.trim() || undefined));
    }, 300);
  }, [router, currentRole, currentPageSize, currentKiller, currentPeriod]);

  return (
    <div className="flex flex-1 flex-col gap-6">
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-3xl font-bold tracking-tight">Leaderboards</h1>
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <Clock className="h-4 w-4" />
          <span>Next update in {minutesLeft} minute{minutesLeft !== 1 ? "s" : ""}</span>
        </div>
      </div>

      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          type="text"
          placeholder="Search by Steam name or Steam ID..."
          value={searchInput}
          onChange={(e) => handleSearchChange(e.target.value)}
          className="pl-9 h-9 text-sm"
        />
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
              <KillerSelect value={currentKiller} killers={killers} onChange={setKiller} />
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

        <div className="grid gap-3 pt-4">
          {items.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-16 text-center">
              {currentKiller && killerRequirements[currentKiller] === "not trackable" ? (
                <>
                  <p className="text-sm text-destructive">{currentKiller} matches cannot be tracked.</p>
                  <p className="mt-1 text-sm text-destructive">This killer does not have a unique Steam achievement to track.</p>
                </>
              ) : (
                <>
                  <p className="text-sm text-muted-foreground">No players found.</p>
                  <p className="mt-1 text-sm text-muted-foreground">
                    Start playing{currentKiller ? ` ${currentKiller}` : currentRole === "survivor" ? " Survivor" : currentRole === "killer" ? " Killer" : ""} and your streaks will appear here.
                  </p>
                </>
              )}
            </div>
          ) : (
            items.map((item) => (
              <Link
                key={item.steamId}
                href={`/leaderboards/${item.steamId}`}
                onClick={() => {
                  if (typeof window !== "undefined") {
                    sessionStorage.setItem("leaderboard_return_url", window.location.href);
                  }
                }}
                className="group rounded-lg border border-border bg-card p-4 hover:border-primary hover:bg-primary/10"
              >
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4 min-w-0">
                    <span className={`text-lg w-8 shrink-0 text-center ${getRankStyle(item.rank)}`}>
                      #{item.rank}
                    </span>
                    {item.avatarUrl ? (
                      <img
                        src={item.avatarUrl}
                        alt={item.displayName}
                        className="h-10 w-10 shrink-0 rounded-full"
                      />
                    ) : (
                      <div className="h-10 w-10 shrink-0 rounded-full bg-muted" />
                    )}
                    <div className="min-w-0">
                      <p className="font-medium break-words">{item.displayName}</p>
                      <p className="text-xs text-muted-foreground break-all">{item.vanityUrl ? `steamcommunity.com/id/${item.vanityUrl}/` : `steamcommunity.com/profiles/${item.steamId}/`}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Flame className="h-4 w-4 text-primary" />
                    <span className="text-lg font-bold">{item.bestStreak}</span>
                  </div>
                </div>
              </Link>
            ))
          )}
        </div>
      </Tabs>

      <PaginationControls currentPage={currentPage} totalPages={totalPages} onPageChange={setPage} />
    </div>
  );
}
