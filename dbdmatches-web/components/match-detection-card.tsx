"use client";

import { useState } from "react";
import { ChevronDown } from "lucide-react";
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/components/ui/collapsible";
import { killerRequirements } from "@/lib/killer-requirements";

interface MatchDetectionCardProps {
  role: string;
  killer?: string;
}

export function MatchDetectionCard({ role, killer }: MatchDetectionCardProps) {
  const [open, setOpen] = useState(false);

  if (role === "killer" && killerRequirements[killer ?? ""] === "not trackable") return null;
  if (role !== "all" && role !== "killer" && role !== "survivor") return null;

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <div className="rounded-lg border border-blue-500/40 bg-blue-500/5 hover:border-blue-500 hover:bg-blue-500/10">
        <CollapsibleTrigger className="flex w-full items-center justify-between p-4 cursor-pointer">
          <p className="text-sm font-medium text-blue-500">How Match Detection Works</p>
          <div className="flex items-center gap-2">
            {!open && <span className="text-xs text-muted-foreground">Click to expand</span>}
            <ChevronDown className={`h-4 w-4 text-blue-500 transition-transform ${open ? "rotate-180" : ""}`} />
          </div>
        </CollapsibleTrigger>
        <CollapsibleContent className="px-4 pb-4">
          <div className="text-xs text-muted-foreground">
            {role === "all" && (
              <>
                <p>1. Only matches played with a <span className="text-foreground font-medium">full-loadout</span> can be tracked.</p>
                <p>2. Killer matches can be tagged as <span className="text-foreground font-medium">&quot;Contaminated&quot;</span> if a non-full-loadout match was played before and will not count towards streaks.</p>
              </>
            )}
            {role === "killer" && (
              <>
                <p>1. Only matches played with a <span className="text-foreground font-medium">full-loadout</span> (4 perks, 2 add-ons, and an offering) can be tracked.</p>
                <p>2. Killer matches can be tagged as <span className="text-foreground font-medium">&quot;Contaminated&quot;</span> if a non-full-loadout match was played before and will not count towards streaks.</p>
                {killer && killerRequirements[killer] && (
                  <p>3. {killer} requires <span className="text-foreground font-medium">{killerRequirements[killer]}</span>, otherwise the match is tagged as <span className="text-foreground font-medium">&quot;Untracked Killer&quot;</span>.</p>
                )}
              </>
            )}
            {role === "survivor" && (
              <p>Only matches played with a <span className="text-foreground font-medium">full-loadout</span> (4 perks, 1 item with 2 add-ons, and an offering) can be tracked.</p>
            )}
          </div>
        </CollapsibleContent>
      </div>
    </Collapsible>
  );
}
