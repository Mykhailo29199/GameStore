# Game Store UI

Angular front end of the Game Store. The .NET API from the `Gamestore` project is expected at `https://localhost:7127`.

## Development

```bash
npm install
npm start
```

The application opens at `http://localhost:4200/`. Requests to `/api` are forwarded to the API by `proxy.conf.json`.

## Build

```bash
npm run build
```

The build is written to `dist/gamestore-ui-app/browser`; the `Gamestore` project serves it from there when the folder exists.

## Tests

```bash
npm test -- --watch=false
```

## Settings

`src/app/core/config/app-settings.ts` holds the API base URL and the tax percent shown on the game page.
