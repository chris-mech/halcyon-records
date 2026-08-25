import type { MetadataRoute } from "next";
import { connection } from "next/server";

import { client } from "@/lib/api/client";
import { SITE_URL } from "@/lib/site-config";

const ALBUM_PAGE_SIZE = 50;

async function getAlbumEntries(): Promise<MetadataRoute.Sitemap> {
  const entries: MetadataRoute.Sitemap = [];
  let page = 1;
  let totalPages = 1;

  do {
    const { data, error } = await client.GET("/api/albums", {
      params: { query: { page, pageSize: ALBUM_PAGE_SIZE } },
    });

    if (error) {
      throw new Error("Failed to load albums for sitemap.");
    }

    entries.push(
      ...data.items.map((album) => ({
        url: `${SITE_URL}/albums/${album.sqid}/${album.titleSlug}`,
      })),
    );

    totalPages =
      data.totalPages ?? Math.ceil(data.totalCount / ALBUM_PAGE_SIZE);
    page += 1;
  } while (page <= totalPages);

  return entries;
}

async function getArtistEntries(): Promise<MetadataRoute.Sitemap> {
  const { data, error } = await client.GET("/api/artists");

  if (error) {
    throw new Error("Failed to load artists for sitemap.");
  }

  return data.map((artist) => ({
    url: `${SITE_URL}/artists/${artist.sqid}/${artist.nameSlug}`,
  }));
}

async function getGenreEntries(): Promise<MetadataRoute.Sitemap> {
  const { data, error } = await client.GET("/api/genres");

  if (error) {
    throw new Error("Failed to load genres for sitemap.");
  }

  return data.map((genre) => ({ url: `${SITE_URL}/genres/${genre.slug}` }));
}

async function getDecadeEntries(): Promise<MetadataRoute.Sitemap> {
  const { data, error } = await client.GET("/api/decades");

  if (error) {
    throw new Error("Failed to load decades for sitemap.");
  }

  return data.map((decade) => ({ url: `${SITE_URL}/decades/${decade.slug}` }));
}

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  await connection();

  const staticEntries: MetadataRoute.Sitemap = [
    { url: SITE_URL },
    { url: `${SITE_URL}/shop` },
    { url: `${SITE_URL}/artists` },
    { url: `${SITE_URL}/genres` },
    { url: `${SITE_URL}/decades` },
  ];

  const [albums, artists, genres, decades] = await Promise.all([
    getAlbumEntries(),
    getArtistEntries(),
    getGenreEntries(),
    getDecadeEntries(),
  ]);

  return [...staticEntries, ...artists, ...genres, ...decades, ...albums];
}
