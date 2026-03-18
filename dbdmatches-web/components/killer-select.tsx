import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

interface KillerSelectProps {
  value: string;
  killers: string[];
  onChange: (killer: string) => void;
}

export function KillerSelect({ value, killers, onChange }: KillerSelectProps) {
  return (
    <Select value={value || "all"} onValueChange={(v) => onChange(v === "all" ? "" : v)}>
      <SelectTrigger className="h-7 w-[180px] text-xs cursor-pointer">
        <SelectValue>{value || "All Killers"}</SelectValue>
      </SelectTrigger>
      <SelectContent position="popper" side="bottom" align="start">
        <SelectItem value="all" className="cursor-pointer text-xs">All Killers</SelectItem>
        {killers.map((k) => (
          <SelectItem key={k} value={k} className="cursor-pointer text-xs">{k}</SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
