export const environment = {
  production: false,
  // Verified against the API's actual `dotnet run` output (Properties/launchSettings.json,
  // "https" profile): "Now listening on: https://localhost:7137" and
  // "Now listening on: http://localhost:5027". We use the plain HTTP port here deliberately:
  // the self-signed HTTPS dev cert isn't trusted on a fresh machine (needs a click-through
  // GUI prompt), and Program.cs's UseHttpsRedirection() was scoped to non-Development so the
  // 307 it would otherwise issue doesn't break the Angular dev server's CORS preflight
  // (browsers refuse to follow a redirect for a cross-origin OPTIONS preflight).
  // See DECISIONS.md.
  apiUrl: 'http://localhost:5027/api/v1',
};
