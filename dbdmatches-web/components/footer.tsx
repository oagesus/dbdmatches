import Link from "next/link";

export function Footer() {
  return (
    <footer className="bg-muted dark:bg-black/30 px-6">
      <div className="mx-auto flex w-full max-w-screen-xl flex-col-reverse items-center gap-3 py-4 md:flex-row md:justify-between">
        <span className="text-xs text-muted-foreground">
          &copy; {new Date().getFullYear()} DBD Matches
        </span>
        <nav className="flex flex-wrap justify-center gap-x-6 gap-y-2">
          <Link href="/privacy-policy" className="text-xs text-muted-foreground hover:text-foreground hover:underline">
            Privacy Policy
          </Link>
          <Link href="/terms-of-service" className="text-xs text-muted-foreground hover:text-foreground hover:underline">
            Terms of Service
          </Link>
          <Link href="/imprint" className="text-xs text-muted-foreground hover:text-foreground hover:underline">
            Imprint
          </Link>
        </nav>
      </div>
    </footer>
  );
}
