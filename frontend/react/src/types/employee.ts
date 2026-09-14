export type Gender = "MALE" | "FEMALE";

export interface Employee {
  id: number;
  name: string;
  email: string;
  age: number;
  gender: Gender;
}

/** Payload for creating a new employee (registration). */
export interface NewEmployee {
  name: string;
  email: string;
  age: number;
  gender: Gender | "";
  password: string;
}

/** Editable subset of an employee. */
export interface EmployeeUpdate {
  name: string;
  email: string;
  age: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

/** Claims carried by the JWT issued on login. */
export interface JwtToken {
  sub: string;
  email: string;
  role: "ADMIN" | "EMPLOYEE";
  exp: number;
}

/** The authenticated user derived from the JWT. */
export interface AuthenticatedEmployee {
  id: number;
  email: string;
  role: "ADMIN" | "EMPLOYEE";
}

export interface LoginResponse {
  accessToken: string;
  tokenType: "Bearer";
  expiresAt: string;
  employee: AuthenticatedEmployee;
}
