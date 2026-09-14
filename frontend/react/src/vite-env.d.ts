/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string;
  readonly VITE_API_MODE?: "demo" | "real";
  readonly VITE_AUTH_ENABLED?: "true" | "false";
  readonly VITE_PROFILE_IMAGES_ENABLED?: "true" | "false";
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
