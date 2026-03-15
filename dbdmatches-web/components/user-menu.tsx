"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { LogOut, Settings, User as UserIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { ThemeToggleIcon } from "@/components/theme-toggle-icon";
import type { User } from "@/lib/auth/get-user";
import { formatNextUpdate, statusLabel, statusColor } from "@/lib/status-utils";
import { useStatusCountdown } from "@/lib/use-countdown";

export function UserMenu({ user: initialUser }: { user: User }) {
  const router = useRouter();
  const [user, setUser] = useState(initialUser);
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
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="cursor-pointer rounded-full">
          <div className="relative inline-block">
            {user.avatarUrl ? (
              <img
                src={user.avatarUrl}
                alt={user.displayName}
                className="h-7 w-7 rounded-full"
              />
            ) : (
              <Avatar className="h-7 w-7">
                <AvatarFallback>
                  <UserIcon className="h-4 w-4" />
                </AvatarFallback>
              </Avatar>
            )}
            <span className="absolute -bottom-0.5 -right-0.5 flex h-3 w-3">
              {user.status === "InGame" && (
                <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-green-400 opacity-75" />
              )}
              <span
                className={`relative inline-flex h-3 w-3 rounded-full border-2 border-background ${statusColor(user.status)}`}
              />
            </span>
          </div>
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
          onClick={() => router.push("/account")}
          className="cursor-pointer"
        >
          <Settings className="mr-2 h-4 w-4" />
          Settings
        </DropdownMenuItem>
        <DropdownMenuSeparator />
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
