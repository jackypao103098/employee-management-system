import axios, { AxiosRequestConfig, AxiosResponse } from "axios";
import { EmployeeUpdate, LoginRequest, NewEmployee } from "../types/employee";

const getAuthConfig = (): AxiosRequestConfig => ({
  headers: {
    Authorization: `Bearer ${localStorage.getItem("access_token")}`,
  },
});

export const getEmployees = async (): Promise<AxiosResponse> =>
  axios.get(
    `${import.meta.env.VITE_API_BASE_URL}/api/v1/employees`,
    getAuthConfig()
  );

export const saveEmployee = async (
  employee: NewEmployee
): Promise<AxiosResponse> =>
  axios.post(
    `${import.meta.env.VITE_API_BASE_URL}/api/v1/employees`,
    employee
  );

export const updateEmployee = async (
  id: number,
  update: EmployeeUpdate
): Promise<AxiosResponse> =>
  axios.put(
    `${import.meta.env.VITE_API_BASE_URL}/api/v1/employees/${id}`,
    update,
    getAuthConfig()
  );

export const deleteEmployee = async (id: number): Promise<AxiosResponse> =>
  axios.delete(
    `${import.meta.env.VITE_API_BASE_URL}/api/v1/employees/${id}`,
    getAuthConfig()
  );

export const login = async (
  usernameAndPassword: LoginRequest
): Promise<AxiosResponse> =>
  axios.post(
    `${import.meta.env.VITE_API_BASE_URL}/api/v1/auth/login`,
    usernameAndPassword
  );

export const uploadEmployeeProfilePicture = async (
  id: number,
  formData: FormData
): Promise<AxiosResponse> =>
  axios.post(
    `${import.meta.env.VITE_API_BASE_URL}/api/v1/employees/${id}/profile-image`,
    formData,
    {
      headers: {
        Authorization: `Bearer ${localStorage.getItem("access_token")}`,
        "Content-Type": "multipart/form-data",
      },
    }
  );

export const employeeProfilePictureUrl = (id: number): string =>
  `${import.meta.env.VITE_API_BASE_URL}/api/v1/employees/${id}/profile-image`;
