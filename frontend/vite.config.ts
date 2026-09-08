import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// The .NET API dev URL (see Api/Properties/launchSettings.json).
const API_TARGET = "http://localhost:5243";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": { target: API_TARGET, changeOrigin: true },
    },
  },
});
