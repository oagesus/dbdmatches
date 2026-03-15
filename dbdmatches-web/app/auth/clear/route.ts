import { NextResponse } from "next/server";

export async function GET() {
  const response = NextResponse.redirect(new URL("/", process.env.NEXT_PUBLIC_URL || "http://localhost:3000"));

  response.cookies.delete("access_token");
  response.cookies.delete("refresh_token");
  response.cookies.delete("token_exp");

  return response;
}
