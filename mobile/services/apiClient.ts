import { storageService } from "./storageService";
import { ERRORS } from "../constants/errors";

// All API calls go through this client — NEVER raw fetch in components or hooks
// tech-preferences 10.4: All API calls through apiClient.ts

const API_BASE_URL =
  process.env.EXPO_PUBLIC_API_URL ?? "http://localhost/api";

type RequestOptions = Omit<RequestInit, "body"> & {
  body?: unknown;
};

async function request<T>(
  path: string,
  options: RequestOptions = {}
): Promise<T> {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  const token = await storageService.getToken();

  let response: Response;
  try {
    response = await fetch(`${API_BASE_URL}${normalizedPath}`, {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...(token !== null ? { Authorization: `Bearer ${token}` } : {}),
        ...options.headers,
      },
      body: options.body !== undefined ? JSON.stringify(options.body) : null,
    });
  } catch {
    throw new Error(ERRORS.NETWORK_ERROR);
  }

  if (!response.ok) {
    // RFC 7807 Problem Details — extract title for user display
    // Never expose raw status codes or stack traces to components (project-context rule 10)
    const problem = await response
      .json()
      .catch(() => ({ title: ERRORS.UNEXPECTED_ERROR }));
    throw new Error(
      (problem as { title?: string }).title ?? ERRORS.UNEXPECTED_ERROR
    );
  }

  if (response.status === 204 || response.headers.get("content-length") === "0") {
    return undefined as unknown as T;
  }

  const contentType = response.headers.get("content-type") ?? "";
  if (!contentType.includes("application/json")) {
    return undefined as unknown as T;
  }

  return response.json() as Promise<T>;
}

export const apiClient = {
  get: <T>(
    path: string,
    options?: Omit<RequestOptions, "method" | "body">
  ) => request<T>(path, { ...options, method: "GET" }),

  post: <T>(
    path: string,
    body?: unknown,
    options?: Omit<RequestOptions, "method" | "body">
  ) => request<T>(path, { ...options, method: "POST", body }),

  put: <T>(
    path: string,
    body?: unknown,
    options?: Omit<RequestOptions, "method" | "body">
  ) => request<T>(path, { ...options, method: "PUT", body }),

  patch: <T>(
    path: string,
    body?: unknown,
    options?: Omit<RequestOptions, "method" | "body">
  ) => request<T>(path, { ...options, method: "PATCH", body }),

  delete: <T>(
    path: string,
    options?: Omit<RequestOptions, "method" | "body">
  ) => request<T>(path, { ...options, method: "DELETE" }),
} as const;
