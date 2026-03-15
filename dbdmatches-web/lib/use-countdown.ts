"use client";

import { useState, useEffect, useRef, useCallback } from "react";

export function useStatusCountdown(initialSeconds: number) {
  const [seconds, setSeconds] = useState(initialSeconds);
  const [status, setStatus] = useState<Record<string, unknown> | null>(null);
  const [isUpdating, setIsUpdating] = useState(false);
  const endTimeRef = useRef(Date.now() + initialSeconds * 1000);
  const isFetchingRef = useRef(false);

  const refetchStatus = useCallback(async () => {
    if (isFetchingRef.current) return;
    isFetchingRef.current = true;
    setIsUpdating(true);

    try {
      const res = await fetch("/api/auth/me");
      if (res.ok) {
        const data = await res.json();
        setStatus(data);
        endTimeRef.current = Date.now() + data.nextUpdateSeconds * 1000;
        setSeconds(data.nextUpdateSeconds);
      }
    } catch {} finally {
      isFetchingRef.current = false;
      setIsUpdating(false);
    }
  }, []);

  useEffect(() => {
    endTimeRef.current = Date.now() + initialSeconds * 1000;
    setSeconds(initialSeconds);
  }, [initialSeconds]);

  useEffect(() => {
    const update = () => {
      const remaining = Math.max(0, Math.ceil((endTimeRef.current - Date.now()) / 1000));
      setSeconds(remaining);

      if (remaining <= 0) {
        refetchStatus();
      }
    };

    const interval = setInterval(update, 1000);

    const handleVisibility = () => {
      if (document.visibilityState === "visible") {
        const remaining = Math.max(0, Math.ceil((endTimeRef.current - Date.now()) / 1000));
        setSeconds(remaining);
        if (remaining <= 0) {
          refetchStatus();
        }
      }
    };
    document.addEventListener("visibilitychange", handleVisibility);

    return () => {
      clearInterval(interval);
      document.removeEventListener("visibilitychange", handleVisibility);
    };
  }, [refetchStatus]);

  return { seconds, updatedUser: status, isUpdating };
}
