import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { ThemeProvider } from "@/components/theme-provider";
import { Navbar } from "@/components/navbar";
import { Footer } from "@/components/footer";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: {
    default: "DBD Matches - Dead by Daylight Match Tracker",
    template: "%s | DBD Matches",
  },
  description:
    "Track your Dead by Daylight matches, analyze your performance, and improve your gameplay with detailed match statistics.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body
        className={`${geistSans.variable} ${geistMono.variable} antialiased min-h-screen flex flex-col`}
      >
        <ThemeProvider
          attribute="class"
          defaultTheme="system"
          enableSystem
          disableTransitionOnChange
        >
          <Navbar />
          <div className="flex flex-1 flex-col px-6 pt-12 pb-20">
            <div className="mx-auto flex w-full max-w-screen-xl flex-1 flex-col">
              {children}
            </div>
          </div>
          <Footer />
        </ThemeProvider>
      </body>
    </html>
  );
}
