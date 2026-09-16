// Mirrors FarmApp.Api.Application.Suppliers.SupplierDtos. Note: Suppliers has NO duplicate-name
// check on the backend (confirmed behaviour from Phase 0b-1, called out again in the Phase 5b-1
// task brief) - the create/edit form deliberately does not attempt to enforce uniqueness
// client-side either, since that would be misleading about what the backend actually allows.
export interface SupplierDto {
  supplierId: number;
  name: string;
  phone: string | null;
  notes: string | null;
  isActive: boolean;
  vatNumber: string | null;
}

export interface CreateSupplierRequest {
  name: string;
  phone: string | null;
  notes: string | null;
  vatNumber: string | null;
}

export interface UpdateSupplierRequest {
  name: string;
  phone: string | null;
  notes: string | null;
  isActive: boolean;
  vatNumber: string | null;
}
