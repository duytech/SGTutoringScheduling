import js from "@eslint/js";
import globals from "globals";
import typescriptEslint from "typescript-eslint";

// Flat config. The only project-specific rule is `id-length`, which enforces the
// "no single-character names, loop indices included" convention from CLAUDE.md so
// it is caught by tooling rather than review.
export default typescriptEslint.config(
  { ignores: ["dist", "node_modules", ".vite", "vite.config.js", "vite.config.d.ts"] },
  js.configs.recommended,
  ...typescriptEslint.configs.recommended,
  {
    files: ["src/**/*.{ts,tsx}"],
    languageOptions: {
      globals: { ...globals.browser },
    },
    rules: {
      "id-length": ["error", { min: 2, exceptions: ["_"], properties: "never" }],
    },
  },
);
