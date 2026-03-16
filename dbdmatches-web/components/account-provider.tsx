"use client";

import { createContext, useCallback, useContext, useState } from "react";
import type { User } from "@/lib/types";

interface AccountContextType {
  user: User;
  refetch: () => Promise<void>;
}

const AccountContext = createContext<AccountContextType | null>(null);

export function AccountProvider({
  children,
  initialUser,
}: {
  children: React.ReactNode;
  initialUser: User;
}) {
  const [user, setUser] = useState<User>(initialUser);

  const refetch = useCallback(async () => {
    try {
      const res = await fetch("/api/auth/me");
      if (res.ok) {
        const data = await res.json();
        setUser(data);
      } else if (res.status === 401) {
        window.location.href = "/auth/clear";
      }
    } catch {
    }
  }, []);

  return (
    <AccountContext.Provider value={{ user, refetch }}>
      {children}
    </AccountContext.Provider>
  );
}

export function useAccount() {
  const context = useContext(AccountContext);
  if (!context) {
    throw new Error("useAccount must be used within an AccountProvider");
  }
  return context;
}
