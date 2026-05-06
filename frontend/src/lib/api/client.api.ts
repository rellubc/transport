import { BASE_URL } from "$lib/constants";
import type { StopStopTime } from "$lib/types/stoptimes.types";
import type { Vehicle, Vehicles } from "$lib/types/vehicles.types";

export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options
  });

  if (!res.ok) throw new Error(`${res.status} ${path}`);

  return await res.json();
}
