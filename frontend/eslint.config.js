import js from "@eslint/js";
import stylistic from "@stylistic/eslint-plugin";
import globals from "globals";
import typescriptEslint from "typescript-eslint";

// Flat config. Two project-specific rules, both enforcing conventions from
// CLAUDE.md so they are caught by tooling rather than review:
//   - `id-length`: no single-character names, loop indices included.
//   - `@stylistic/padding-line-between-statements`: a blank line above every
//     `return` that is not the first statement in its block.
export default typescriptEslint.config(
  { ignores: ["dist", "node_modules", ".vite", "vite.config.js", "vite.config.d.ts"] },
  js.configs.recommended,
  ...typescriptEslint.configs.recommended,
  {
    files: ["src/**/*.{ts,tsx}"],
    plugins: { "@stylistic": stylistic },
    languageOptions: {
      globals: { ...globals.browser },
    },
    rules: {
      "id-length": ["error", { min: 2, exceptions: ["_"], properties: "never" }],
      "@stylistic/padding-line-between-statements": [
        "error",
        { blankLine: "always", prev: "*", next: "return" },
      ],
    },
  },
);
