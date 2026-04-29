import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import { cn } from "@/lib/utils";
import { QueryProvider, CoachProvider, AuthProvider } from "@/providers";

const inter = Inter({ subsets: ["latin"], variable: "--font-sans" });

export const metadata: Metadata = {
  title: "CoachPlatform - Dashboard",
  description: "Coaching platform for strength training coaches",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className={cn("font-sans", inter.variable)}>
      <body className={cn("antialiased bg-slate-50", inter.className)}>
        <QueryProvider>
          <AuthProvider>
            <CoachProvider>
              {children}
            </CoachProvider>
          </AuthProvider>
        </QueryProvider>
      </body>
    </html>
  );
}

