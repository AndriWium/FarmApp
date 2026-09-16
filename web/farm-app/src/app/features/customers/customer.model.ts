// Mirrors FarmApp.Api.Application.Customers.CustomerDtos + FarmApp.Domain.Enums.CustomerType.
// This is the first entity in the codebase holding real personal information (a person's name and
// phone number) - CustomersController's own comment on this is preserved here as a reminder: all
// writes go through CustomersApiService's POST/PUT bodies only, never a route or query parameter,
// and nothing here should ever be tempted to add a "search by phone" query-param endpoint.
// No duplicate-name check on the backend (same as Supplier) - not enforced client-side either,
// per the Phase 5b-1 precedent (see supplier.model.ts's comment).
export type CustomerType = 'Retail' | 'Wholesale' | 'Account';

export const CUSTOMER_TYPES: CustomerType[] = ['Retail', 'Wholesale', 'Account'];

export interface CustomerDto {
  customerId: number;
  name: string;
  phone: string | null;
  type: CustomerType;
  priceListId: number;
  creditLimit: number | null;
  isActive: boolean;
}

export interface CreateCustomerRequest {
  name: string;
  phone: string | null;
  type: CustomerType;
  priceListId: number;
  creditLimit: number | null;
}

export interface UpdateCustomerRequest {
  name: string;
  phone: string | null;
  type: CustomerType;
  priceListId: number;
  creditLimit: number | null;
  isActive: boolean;
}
