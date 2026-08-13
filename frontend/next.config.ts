import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  reactCompiler: true,
  cacheComponents: true,
  output: "standalone",
  images: {
    remotePatterns: [
      { protocol: "https", hostname: "coverartarchive.org" },
      { protocol: "https", hostname: "archive.org" },
      { protocol: "https", hostname: "*.archive.org" },
    ],
  },
};

export default nextConfig;
