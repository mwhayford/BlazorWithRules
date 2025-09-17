import js from "@eslint/js";

export default [
    js.configs.recommended,
    {
        // Global ignores (applied to all configurations)
        ignores: [
            // Third-party libraries and vendor files
            "**/wwwroot/lib/**",
            "node_modules/**",
            "**/*.min.js",
            "**/*.min.css",

            // Build output directories
            "**/bin/**",
            "**/obj/**",
            "**/dist/**",
            "**/build/**",
            "**/publish/**",

            // Package manager files
            "package-lock.json",
            "*.lock",

            // Generated files
            "**/*.generated.js",
            "**/*.designer.js",

            // IDE and system files
            ".vs/**",
            ".vscode/**",
            "*.suo",
            "*.user",
            ".DS_Store",
            "Thumbs.db",

            // CSS files (not JavaScript)
            "**/*.css",
            "**/*.razor.css",
        ],
    },
    {
        files: ["**/*.js", "**/*.mjs"],
        languageOptions: {
            ecmaVersion: 2022,
            sourceType: "module",
            globals: {
                console: "readonly",
                window: "readonly",
                document: "readonly",
                navigator: "readonly",
                fetch: "readonly",
                URL: "readonly",
                URLSearchParams: "readonly",
            },
        },
        rules: {
            "no-unused-vars": [
                "error",
                {
                    argsIgnorePattern: "^_",
                    varsIgnorePattern: "^_",
                },
            ],
            "no-console": "warn",
            "prefer-const": "error",
            "no-var": "error",
            eqeqeq: ["error", "always"],
            curly: ["error", "all"],
            "brace-style": ["error", "1tbs"],
            indent: ["error", 4],
            quotes: ["error", "double"],
            semi: ["error", "always"],
        },
    },
    {
        files: ["**/*.css"],
        rules: {
            // CSS-specific rules would go here if using a CSS linter
        },
    },
];
