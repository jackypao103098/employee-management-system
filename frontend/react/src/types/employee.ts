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
  username: string;
  password: string;
}

/** Claims carried by the JWT issued on login. */
export interface JwtToken {
  sub: string;
  scopes: string[];
  exp: number;
}

/** The authenticated user derived from the JWT. */
export interface AuthenticatedEmployee {
  username: string;
  roles: string[];
}
