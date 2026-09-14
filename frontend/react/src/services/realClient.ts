import axios, {AxiosHeaders, AxiosResponse} from "axios";
import {
  Employee,
  EmployeeUpdate,
  LoginRequest,
  LoginResponse,
  NewEmployee,
} from "../types/employee";
import {clearAccessToken, getAccessToken} from "./authToken";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
});

api.interceptors.request.use((config) => {
  const token = getAccessToken();
  if (token) {
    const headers = AxiosHeaders.from(config.headers as AxiosHeaders | undefined);
    headers.set("Authorization", `Bearer ${token}`);
    config.headers = headers;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && getAccessToken()) {
      clearAccessToken();
      if (window.location.pathname !== "/") {
        window.location.assign("/");
      }
    }
    return Promise.reject(error);
  }
);

export const getEmployees = async (): Promise<AxiosResponse<Employee[]>> =>
  api.get("/api/v1/employees");

export const saveEmployee = async (
  employee: NewEmployee
): Promise<AxiosResponse<Employee>> =>
  api.post("/api/v1/employees", employee);

export const updateEmployee = async (
  id: number,
  update: EmployeeUpdate
): Promise<AxiosResponse<Employee>> =>
  api.put(`/api/v1/employees/${id}`, update);

export const deleteEmployee = async (id: number): Promise<AxiosResponse<void>> =>
  api.delete(`/api/v1/employees/${id}`);

export const login = async (
  credentials: LoginRequest
): Promise<AxiosResponse<LoginResponse>> =>
  api.post("/api/v1/auth/login", credentials);

export const uploadEmployeeProfilePicture = async (
  id: number,
  formData: FormData
): Promise<AxiosResponse> =>
  api.post(`/api/v1/employees/${id}/profile-image`, formData);

export const employeeProfilePictureUrl = (id: number): string =>
  `${import.meta.env.VITE_API_BASE_URL}/api/v1/employees/${id}/profile-image`;
