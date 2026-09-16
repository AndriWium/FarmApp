// Mirrors FarmApp.Api.Application.Auth.AuthDtos (LoginRequest/RefreshRequest/TokenResponse).
export interface LoginRequest {
  userName: string;
  password: string;
}

export interface RefreshRequest {
  refreshToken: string;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
}

// The API's TokenService (src/FarmApp.Api/Application/Auth/TokenService.cs) builds the JWT
// from `new Claim(ClaimTypes.NameIdentifier, ...)` / `ClaimTypes.Name` / `ClaimTypes.Role`
// passed straight into `new JwtSecurityToken(claims: ...)` - NOT through a ClaimsIdentity /
// SecurityTokenDescriptor, so JwtSecurityTokenHandler's outbound short-name mapping never
// applies. Confirmed by decoding a real token from POST /api/v1/auth/login: the payload keys
// are the full legacy WS-Federation claim URIs below, not the "sub"/"unique_name"/"role"
// short names an older reference guide assumed. See DECISIONS.md.
export const JWT_CLAIM_USER_ID = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
export const JWT_CLAIM_USER_NAME = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
export const JWT_CLAIM_ROLE = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

export interface JwtPayload {
  [JWT_CLAIM_USER_ID]: string;
  [JWT_CLAIM_USER_NAME]: string;
  [JWT_CLAIM_ROLE]: string;
  exp: number;
  iss: string;
  aud: string;
}

export interface CurrentUser {
  userId: string;
  userName: string;
  role: string;
}

export function toCurrentUser(payload: JwtPayload): CurrentUser {
  return {
    userId: payload[JWT_CLAIM_USER_ID],
    userName: payload[JWT_CLAIM_USER_NAME],
    role: payload[JWT_CLAIM_ROLE],
  };
}
