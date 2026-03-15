"use client";

import { useCallback, useEffect, useRef } from "react";

function getTokenExp(): number | null {
  const match = document.cookie.match(/(?:^|;\s*)token_exp=(\d+)/);
  return match ? parseInt(match[1], 10) : null;
}

export function AuthGuardian() {
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const scheduleRefresh = useCallback(() => {
    if (timerRef.current) {
      clearTimeout(timerRef.current);
    }

    const exp = getTokenExp();
    if (!exp) return;

    const now = Math.floor(Date.now() / 1000);
    const secondsUntilExpiry = exp - now;
    const refreshIn = Math.max((secondsUntilExpiry - 30) * 1000, 0);

    timerRef.current = setTimeout(async () => {
      try {
        const res = await fetch("/api/auth/refresh", { method: "POST" });
        if (res.ok) {
          scheduleRefresh();
        }
      } catch {
      }
    }, refreshIn);
  }, []);

  useEffect(() => {
    scheduleRefresh();
    return () => {
      if (timerRef.current) {
        clearTimeout(timerRef.current);
      }
    };
  }, [scheduleRefresh]);

  return null;
}
