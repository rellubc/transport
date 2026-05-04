import { BASE_URL } from "$lib/constants";

export async function client<T>(path: string, options: RequestInit = {}): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options
  });

  if (!res.ok) throw new Error(`${res.status} ${path}`);

  return await res.json();
}