import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

const periodLabels: Record<string, string> = {
  all: "All Time",
  "30d": "Last 30 Days",
  "90d": "Last 90 Days",
  "1y": "Last Year",
};

interface PeriodSelectProps {
  value: string;
  onChange: (period: string) => void;
}

export function PeriodSelect({ value, onChange }: PeriodSelectProps) {
  return (
    <Select value={value || "all"} onValueChange={onChange}>
      <SelectTrigger className="h-7 w-[130px] text-xs cursor-pointer">
        <SelectValue>{periodLabels[value || "all"]}</SelectValue>
      </SelectTrigger>
      <SelectContent position="popper" side="bottom" align="start">
        {Object.entries(periodLabels).map(([key, label]) => (
          <SelectItem key={key} value={key} className="cursor-pointer text-xs">{label}</SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
