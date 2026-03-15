import Link from "next/link";
import { ThemeToggle } from "@/components/theme-toggle";
import { Button } from "@/components/ui/button";
import { MobileMenu } from "@/components/mobile-menu";
import { MobileMenuAuth } from "@/components/mobile-menu-auth";
import { UserMenu } from "@/components/user-menu";
import { getUser } from "@/lib/auth/get-user";
import { SteamIcon } from "@/components/icons/steam-icon";

export async function Navbar() {
  const user = await getUser();

  return (
    <header className="sticky top-0 z-50 w-full bg-background/80 backdrop-blur-sm px-6">
      <div className="mx-auto flex h-16 w-full max-w-screen-xl items-center justify-between">
        <Link
          href={user ? "/dashboard" : "/"}
          className="flex items-center gap-3 text-2xl font-light"
        >
          DBD Matches
        </Link>
        <div className="flex items-center gap-2">
          {user ? (
            <>
              <Button variant="ghost" asChild className="hidden lg:inline-flex">
                <Link href="/matches">Matches</Link>
              </Button>
              <Button variant="ghost" asChild className="hidden lg:inline-flex">
                <Link href="/stats">Statistics</Link>
              </Button>
              <Button variant="ghost" asChild className="hidden lg:inline-flex">
                <Link href="/history">History</Link>
              </Button>
              <div className="hidden lg:flex items-center">
                <UserMenu user={user} />
              </div>
              <div className="lg:hidden">
                <MobileMenuAuth user={user} />
              </div>
            </>
          ) : (
            <>
              <Button variant="ghost" asChild className="hidden lg:inline-flex">
                <Link href="/matches">Matches</Link>
              </Button>
              <Button variant="ghost" asChild className="hidden lg:inline-flex">
                <Link href="/stats">Statistics</Link>
              </Button>
              <Button variant="ghost" asChild className="hidden lg:inline-flex">
                <Link href="/history">History</Link>
              </Button>
              <Button asChild className="hidden lg:inline-flex">
                <a href={`${process.env.NEXT_PUBLIC_API_URL || "http://localhost:5100"}/api/auth/steam/login`}>
                  <SteamIcon className="h-4 w-4" />
                  Sign In
                </a>
              </Button>
              <div className="hidden lg:flex items-center gap-1">
                <ThemeToggle />
              </div>
              <div className="lg:hidden">
                <MobileMenu />
              </div>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
