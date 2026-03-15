"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Swords, BarChart3, History, Settings, LogOut } from "lucide-react";
import { formatNextUpdate, statusLabel, statusColor } from "@/lib/status-utils";
import { useStatusCountdown } from "@/lib/use-countdown";

import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { ThemeToggleIcon } from "@/components/theme-toggle-icon";
import type { User } from "@/lib/auth/get-user";

export function MobileMenuAuth({ user: initialUser }: { user: User }) {
  const router = useRouter();
  const [user, setUser] = useState(initialUser);
  const [isOpen, setIsOpen] = useState(false);
  const { seconds, updatedUser, isUpdating } = useStatusCountdown(user.nextUpdateSeconds);

  useEffect(() => {
    if (updatedUser) {
      setUser(updatedUser as unknown as User);
    }
  }, [updatedUser]);

  async function handleLogout() {
    await fetch("/api/auth/logout", {
      method: "POST",
      credentials: "include",
    });
    window.location.href = "/auth/clear";
  }

  return (
    <DropdownMenu open={isOpen} onOpenChange={setIsOpen}>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="cursor-pointer hover:text-current aria-expanded:text-current">
          <div className="relative h-5 w-5">
            <span
              className={`absolute left-0 block h-0.5 w-5 bg-current transition-all duration-300 ${
                isOpen ? "top-[9px] rotate-45" : "top-1"
              }`}
            />
            <span
              className={`absolute left-0 top-[9px] block h-0.5 w-5 bg-current transition-all duration-300 ${
                isOpen ? "opacity-0" : "opacity-100"
              }`}
            />
            <span
              className={`absolute left-0 block h-0.5 w-5 bg-current transition-all duration-300 ${
                isOpen ? "top-[9px] -rotate-45" : "top-[14px]"
              }`}
            />
          </div>
          <span className="sr-only">Menu</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-56">
        <DropdownMenuLabel className="font-normal">
          <p className="text-sm font-medium">{user.displayName}</p>
          <div className="flex items-center gap-1.5 text-xs text-muted-foreground">
            <span className={`h-2 w-2 rounded-full ${statusColor(user.status)}`} />
            {statusLabel(user.status)} · {isUpdating ? "updating..." : `updates in ${formatNextUpdate(seconds)}`}
          </div>
        </DropdownMenuLabel>
        <DropdownMenuSeparator />
        <DropdownMenuItem
          onClick={() => router.push("/matches")}
          className="cursor-pointer"
        >
          <Swords className="mr-2 h-4 w-4" />
          Matches
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={() => router.push("/stats")}
          className="cursor-pointer"
        >
          <BarChart3 className="mr-2 h-4 w-4" />
          Statistics
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={() => router.push("/history")}
          className="cursor-pointer"
        >
          <History className="mr-2 h-4 w-4" />
          History
        </DropdownMenuItem>
        <DropdownMenuSeparator />
        <DropdownMenuItem
          onClick={() => router.push("/account")}
          className="cursor-pointer"
        >
          <Settings className="mr-2 h-4 w-4" />
          Settings
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={handleLogout}
          className="cursor-pointer focus:bg-destructive/10 focus:text-destructive"
        >
          <LogOut className="mr-2 h-4 w-4 text-destructive!" />
          Sign Out
        </DropdownMenuItem>
        <DropdownMenuSeparator />
        <div className="flex items-center justify-center">
          <ThemeToggleIcon />
        </div>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
