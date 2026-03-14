import Link from "next/link";
import { ThemeToggle } from "@/components/theme-toggle";
import { Button } from "@/components/ui/button";
import { MobileMenu } from "@/components/mobile-menu";

export function Navbar() {
  return (
    <header className="sticky top-0 z-50 w-full bg-background/80 backdrop-blur-sm px-6">
      <div className="mx-auto flex h-16 w-full max-w-screen-xl items-center justify-between">
        <Link href="/" className="flex items-center gap-3 text-2xl font-light">
          DBD Matches
        </Link>
        <div className="flex items-center gap-2">
          <Button variant="ghost" asChild className="hidden lg:inline-flex">
            <Link href="/matches">Matches</Link>
          </Button>
          <Button variant="ghost" asChild className="hidden lg:inline-flex">
            <Link href="/stats">Statistics</Link>
          </Button>
          <Button variant="ghost" asChild className="hidden lg:inline-flex">
            <Link href="/history">History</Link>
          </Button>
          <div className="hidden lg:block w-px h-5 bg-border" />
          <Button variant="ghost" asChild className="hidden lg:inline-flex">
            <Link href="/auth/login">Login</Link>
          </Button>
          <Button asChild className="hidden lg:inline-flex">
            <Link href="/auth/register">Register</Link>
          </Button>
          <div className="hidden lg:flex items-center gap-1">
            <ThemeToggle />
          </div>
          <div className="lg:hidden">
            <MobileMenu />
          </div>
        </div>
      </div>
    </header>
  );
}
