# MentalHealth

This application provides mental health support services, including APIs for professionals and AI-driven emotional assistance.

## AI Provider Configuration

You can choose between OpenAI and Google Gemini as the backend for the conversational assistant. The code uses `IAiSupportService` to abstract the provider; only one implementation is registered based on configuration.

In `appsettings.json` set `AiProvider` to either `OpenAI` (default) or `Google` and fill the corresponding section (`OpenAI` or `Google`) with credentials and options.

Example configuration:

```json
  "AiProvider": "OpenAI",
  "OpenAI": { /* ... */ },
  "Google": { /* ... */ }
```

Switching providers requires no code changes beyond adding the key and updating the setting. The `GoogleGeminiSupportService` contains a skeleton implementation that mimics an HTTP call to the Gemini endpoint.
