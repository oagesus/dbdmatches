import Link from "next/link";
import { Button } from "@/components/ui/button";
import {
  Swords,
  BarChart3,
  History,
  GithubIcon,
  Star,
} from "lucide-react";
import { SteamIcon } from "@/components/icons/steam-icon";

export default function Home() {
  return (
    <div className="flex flex-1 flex-col">
      <section className="relative flex flex-col items-start pb-6 text-left md:items-center md:text-center md:py-16">
        <div className="pointer-events-none absolute inset-0 hidden overflow-hidden md:block">
          <div className="absolute left-0 top-0 flex flex-col gap-4 opacity-20">
            {Array.from({ length: 10 }).map((_, row) => (
              <div key={row} className="flex gap-4">
                {Array.from({ length: 14 }).map((_, col) => (
                  <div key={col} className="h-1 w-1 rounded-full bg-muted-foreground" />
                ))}
              </div>
            ))}
          </div>
          <div className="absolute right-0 top-1/3 flex -translate-y-1/2 flex-col items-end gap-4 opacity-20">
            {Array.from({ length: 10 }).map((_, row) => (
              <div key={row} className="flex gap-4">
                {Array.from({ length: 10 }).map((_, col) => (
                  <div key={col} className="h-1 w-1 rounded-full bg-muted-foreground" />
                ))}
              </div>
            ))}
          </div>
        </div>
        <h1 className="text-4xl font-bold tracking-tight md:text-6xl">
          Track Your{" "}
          <span className="text-primary">Dead by Daylight</span>
          <br />
          Matches
        </h1>

        <p className="mt-6 md:mt-10 max-w-2xl text-lg text-muted-foreground md:text-xl">
          Automatically track your matches via Steam, analyze your performance
          as Killer or Survivor, and watch your progress over time.
        </p>

        <div className="mt-6 md:mt-10 flex w-full flex-col items-center justify-center gap-4 md:w-auto md:flex-row">
          <Button size="lg" className="h-15 w-full px-10 text-lg md:w-48" asChild>
            <a href={`${process.env.NEXT_PUBLIC_API_URL || "http://localhost:5100"}/api/auth/steam/login`}><SteamIcon className="h-5 w-5" />Get Started</a>
          </Button>
          <Button size="lg" variant="outline" className="h-15 w-full px-10 text-lg md:w-48" asChild>
            <Link href="/matches">View Matches</Link>
          </Button>
        </div>
      </section>

      <section className="py-6 md:py-16">
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          <div className="group rounded-lg border border-border bg-card p-6 hover:border-primary hover:bg-primary/10">
            <Swords className="h-10 w-10 text-primary" />
            <div className="mt-4 inline-block">
              <h3 className="text-lg font-semibold">Automatic Tracking</h3>
              <div className="mt-2 h-0.5 w-12 bg-primary transition-all duration-200 group-hover:w-full" />
            </div>
            <p className="mt-3 text-sm text-muted-foreground">
              Just link your Steam account. Matches are detected and recorded automatically.
            </p>
          </div>

          <div className="group rounded-lg border border-border bg-card p-6 hover:border-primary hover:bg-primary/10">
            <BarChart3 className="h-10 w-10 text-primary" />
            <div className="mt-4 inline-block">
              <h3 className="text-lg font-semibold">Performance Stats</h3>
              <div className="mt-2 h-0.5 w-12 bg-primary transition-all duration-200 group-hover:w-full" />
            </div>
            <p className="mt-3 text-sm text-muted-foreground">
              Analyze your win rates, kill rates, and escape rates with detailed charts and breakdowns.
            </p>
          </div>

          <div className="group rounded-lg border border-border bg-card p-6 hover:border-primary hover:bg-primary/10">
            <History className="h-10 w-10 text-primary" />
            <div className="mt-4 inline-block">
              <h3 className="text-lg font-semibold">Match History</h3>
              <div className="mt-2 h-0.5 w-12 bg-primary transition-all duration-200 group-hover:w-full" />
            </div>
            <p className="mt-3 text-sm text-muted-foreground">
              Browse your full match history with filters by killer, result, and date range.
            </p>
          </div>

        </div>
      </section>

      <section className="flex flex-col items-center py-6 md:py-16 text-center">
        <div className="flex items-center gap-3">
          <GithubIcon className="h-10 w-10" />
          <h2 className="text-3xl font-bold">Open Source</h2>
        </div>
        <p className="mt-6 max-w-xl text-muted-foreground">
          DBDMatches is fully open source and free to use, licensed under the{" "}
          <a
            href="https://github.com/oagesus/dbdmatches/blob/main/LICENSE"
            target="_blank"
            rel="noopener noreferrer"
            className="whitespace-nowrap text-primary hover:underline"
          >
            AGPL-3.0 License
          </a>{" "}
          on{" "}
          <a
            href="https://github.com/oagesus/dbdmatches"
            target="_blank"
            rel="noopener noreferrer"
            className="text-primary hover:underline"
          >
            GitHub
          </a>
          .
        </p>
        <Button size="lg" className="mt-8 h-13 gap-2 bg-primary/10 px-8 text-base text-primary hover:bg-primary/20" asChild>
          <a
            href="https://github.com/oagesus/dbdmatches"
            target="_blank"
            rel="noopener noreferrer"
          >
            <Star className="h-5 w-5" />
            Star on GitHub
          </a>
        </Button>
        <a
          href="https://github.com/oagesus/dbdmatches"
          target="_blank"
          rel="noopener noreferrer"
          className="group mt-6 inline-flex flex-col items-center text-sm text-muted-foreground hover:text-primary"
        >
          Check out the source code
          <div className="mt-4 h-0.5 w-12 bg-primary transition-all duration-200 group-hover:w-full" />
        </a>
      </section>
    </div>
  );
}
