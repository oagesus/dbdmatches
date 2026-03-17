import type { User } from "@/lib/auth/get-user";

export function formatNextUpdate(seconds: number): string {
  if (seconds <= 0) return "1s";
  const m = Math.floor(seconds / 60);
  const s = seconds % 60;
  if (m > 0 && s > 0) return `${m}m ${s}s`;
  if (m > 0) return `${m}m`;
  return `${s}s`;
}

export function statusLabel(status: User["status"]): string {
  if (status === "InGame") return "In Game";
  return status;
}

export function statusColor(status: User["status"]): string {
  if (status === "InGame") return "bg-green-500";
  if (status === "Online") return "bg-blue-500";
  return "bg-gray-400";
}
