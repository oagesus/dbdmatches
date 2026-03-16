export interface User {
  publicId: string;
  steamId: string;
  displayName: string;
  avatarUrl: string | null;
  status: "Offline" | "Online" | "InGame";
  nextUpdateSeconds: number;
  createdAt: string;
}

export interface KillerMatchDetails {
  killerName: string;
  sacrifices: number;
  kills: number;
  powerStat1: number;
  powerStat1Label: string;
  powerStat2: number;
  powerStat2Label: string | null;
  powerStat3: number;
  powerStat3Label: string | null;
}

export interface SurvivorMatchDetails {
  escaped: boolean;
  hatchEscape: boolean;
  generatorsCompleted: number;
}

export interface MatchHistoryItem {
  publicId: string;
  role: "killer" | "survivor";
  result: "Win" | "Loss" | "Draw";
  playedAt: string;
  bloodpointsEarned: number;
  killer: KillerMatchDetails | null;
  survivor: SurvivorMatchDetails | null;
}

export interface StreakData {
  overall: { current: number; best: number };
  killer: { current: number; best: number };
  survivor: { current: number; best: number };
  killers: { killer: string; current: number; best: number }[];
}
