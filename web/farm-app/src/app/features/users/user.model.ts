// Mirrors FarmApp.Api.Application.Users.UserDtos.
export interface UserDto {
  appUserId: number;
  userName: string;
  role: string;
  isActive: boolean;
}

export interface CreateUserRequest {
  userName: string;
  password: string;
  role: string;
}

export interface SetUserActiveRequest {
  isActive: boolean;
}

export interface UpdateUserRoleRequest {
  role: string;
}

export interface ResetPasswordRequest {
  newPassword: string;
}

// Matches AppUser.Role (Domain/Entities/AppUser.cs) and CreateUserRequestValidator/
// UpdateUserRoleRequestValidator's closed set - the only three roles this app recognizes.
export const APP_USER_ROLES = ['Owner', 'Cashier', 'Worker'] as const;
export type AppUserRole = (typeof APP_USER_ROLES)[number];

// Honest, current-state description of what each role can actually do in this app right now
// (doc 13's policy table + DECISIONS.md's confirmed design decision) - shown next to the role
// dropdown on the create form so "what permissions that user can have" has a real answer.
// Cashier and Worker are deliberately worded identically on purpose: they ARE functionally
// identical today (both fall through to the deny-by-default-but-any-authenticated-user-passes
// fallback policy for everything operational), so the UI doesn't imply a distinction that
// doesn't exist.
export const APP_USER_ROLE_DESCRIPTIONS: Record<AppUserRole, string> = {
  Owner:
    'Full access - selling, farming capture, and stock, plus master data (grades, crops, products, ' +
    'suppliers, etc.), reports, user management, and month-end close.',
  Cashier:
    'Day-to-day operational access - selling at the till, farming capture, and stock movements. ' +
    'No master data, no reports, no month-end close, no user management. (Currently identical to ' +
    'Worker - this app does not yet distinguish till duties from farm duties.)',
  Worker:
    'Day-to-day operational access - selling at the till, farming capture, and stock movements. ' +
    'No master data, no reports, no month-end close, no user management. (Currently identical to ' +
    'Cashier - this app does not yet distinguish till duties from farm duties.)',
};
