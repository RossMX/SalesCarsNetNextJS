import type { Metadata } from "next";
import "./globals.css";
import NavBar from "./nav/NavBar";
import ToasterProvider from "./providers/ToasterProvider";
import SignalRProvider from "./providers/SignalRProvider";
import { getCurrentUser } from "./actions/authActions";

export const metadata: Metadata = {
  title: "Carsties",
  description: "Car sales platform",
};

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const user = await getCurrentUser(); // Fetch the current user from your authentication provider
  return (
    <html lang="en">
      <body>
        <NavBar />
        <ToasterProvider />
        <main className="container mx-auto px-5 pt-10">
          <SignalRProvider user={user}>
            {children}
          </SignalRProvider>
        </main>
      </body>
    </html>
  );
}
