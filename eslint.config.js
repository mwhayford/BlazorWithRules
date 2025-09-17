import js from "@eslint/js";

export default [
    js.configs.recommended,
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
