import { AxiosResponse } from "axios";
import {
  Employee,
  EmployeeUpdate,
  LoginRequest,
  NewEmployee,
} from "../types/employee";

const EMPLOYEES_STORAGE_KEY = "employee-management-demo-employees-v1";
const IMAGE_STORAGE_PREFIX = "employee-management-demo-image-";
const DEMO_EMAIL = "demo@jackypao.com";
const DEMO_PASSWORD = "password";
const MAX_STORED_IMAGE_BYTES = 1024 * 1024;

const seedEmployees: Employee[] = [
  {
    id: 1,
    name: "王小明",
    email: DEMO_EMAIL,
    age: 30,
    gender: "MALE",
  },
  {
    id: 2,
    name: "陳美玲",
    email: "mei-ling.chen@example.com",
    age: 27,
    gender: "FEMALE",
  },
  {
    id: 3,
    name: "林志豪",
    email: "chih-hao.lin@example.com",
    age: 35,
    gender: "MALE",
  },
];

const wait = (milliseconds = 250) =>
  new Promise((resolve) => window.setTimeout(resolve, milliseconds));

const response = <T>(
  data: T,
  headers: Record<string, string> = {}
): AxiosResponse<T> =>
  ({
    data,
    status: 200,
    statusText: "OK",
    headers,
    config: {},
  } as AxiosResponse<T>);

const demoError = (code: string, message: string) => ({
  code,
  response: {
    data: {
      message,
    },
  },
});

const encodeJwtPart = (value: object) =>
  btoa(JSON.stringify(value))
    .replace(/\+/g, "-")
    .replace(/\//g, "_")
    .replace(/=/g, "");

const createDemoToken = (username: string) => {
  const nowInSeconds = Math.floor(Date.now() / 1000);
  const header = encodeJwtPart({ alg: "none", typ: "JWT" });
  const payload = encodeJwtPart({
    sub: username,
    scopes: ["ROLE_USER"],
    iat: nowInSeconds,
    exp: nowInSeconds + 8 * 60 * 60,
  });

  return `${header}.${payload}.`;
};

const readEmployees = (): Employee[] => {
  const storedEmployees = localStorage.getItem(EMPLOYEES_STORAGE_KEY);

  if (!storedEmployees) {
    localStorage.setItem(
      EMPLOYEES_STORAGE_KEY,
      JSON.stringify(seedEmployees)
    );
    return structuredClone(seedEmployees);
  }

  try {
    return JSON.parse(storedEmployees) as Employee[];
  } catch {
    localStorage.setItem(
      EMPLOYEES_STORAGE_KEY,
      JSON.stringify(seedEmployees)
    );
    return structuredClone(seedEmployees);
  }
};

const writeEmployees = (employees: Employee[]) => {
  localStorage.setItem(EMPLOYEES_STORAGE_KEY, JSON.stringify(employees));
};

const assertUniqueEmail = (email: string, ignoredEmployeeId?: number) => {
  const duplicate = readEmployees().some(
    (employee) =>
      employee.id !== ignoredEmployeeId &&
      employee.email.toLowerCase() === email.toLowerCase()
  );

  if (duplicate) {
    throw demoError("DUPLICATE_EMAIL", "此 Email 已被其他員工使用");
  }
};

export const getEmployees = async (): Promise<AxiosResponse<Employee[]>> => {
  await wait();
  return response(readEmployees());
};

export const saveEmployee = async (
  employee: NewEmployee
): Promise<AxiosResponse<Employee>> => {
  await wait();
  assertUniqueEmail(employee.email);

  const employees = readEmployees();
  const createdEmployee: Employee = {
    id: Math.max(0, ...employees.map(({ id }) => id)) + 1,
    name: employee.name,
    email: employee.email,
    age: Number(employee.age),
    gender: employee.gender === "FEMALE" ? "FEMALE" : "MALE",
  };

  writeEmployees([...employees, createdEmployee]);

  return response(createdEmployee, {
    authorization: createDemoToken(createdEmployee.email),
  });
};

export const updateEmployee = async (
  id: number,
  update: EmployeeUpdate
): Promise<AxiosResponse<Employee>> => {
  await wait();
  assertUniqueEmail(update.email, id);

  const employees = readEmployees();
  const employeeIndex = employees.findIndex((employee) => employee.id === id);

  if (employeeIndex < 0) {
    throw demoError("NOT_FOUND", "找不到指定的員工");
  }

  const updatedEmployee = {
    ...employees[employeeIndex],
    ...update,
    age: Number(update.age),
  };
  employees[employeeIndex] = updatedEmployee;
  writeEmployees(employees);

  return response(updatedEmployee);
};

export const deleteEmployee = async (
  id: number
): Promise<AxiosResponse<void>> => {
  await wait();
  const employees = readEmployees();

  if (!employees.some((employee) => employee.id === id)) {
    throw demoError("NOT_FOUND", "找不到指定的員工");
  }

  writeEmployees(employees.filter((employee) => employee.id !== id));
  localStorage.removeItem(`${IMAGE_STORAGE_PREFIX}${id}`);
  return response(undefined);
};

export const login = async ({
  username,
  password,
}: LoginRequest): Promise<AxiosResponse<void>> => {
  await wait(400);

  if (username !== DEMO_EMAIL || password !== DEMO_PASSWORD) {
    throw demoError(
      "INVALID_CREDENTIALS",
      "展示模式請使用 demo@jackypao.com / password"
    );
  }

  return response(undefined, {
    authorization: createDemoToken(username),
  });
};

export const uploadEmployeeProfilePicture = async (
  id: number,
  formData: FormData
): Promise<AxiosResponse<void>> => {
  const file = formData.get("file");

  if (!(file instanceof File)) {
    throw demoError("INVALID_FILE", "請選擇圖片檔案");
  }

  if (file.size > MAX_STORED_IMAGE_BYTES) {
    throw demoError("FILE_TOO_LARGE", "展示模式的圖片上限為 1 MB");
  }

  const imageDataUrl = await new Promise<string>((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(String(reader.result));
    reader.onerror = () =>
      reject(demoError("FILE_READ_ERROR", "無法讀取圖片檔案"));
    reader.readAsDataURL(file);
  });

  try {
    localStorage.setItem(`${IMAGE_STORAGE_PREFIX}${id}`, imageDataUrl);
  } catch {
    throw demoError("STORAGE_FULL", "瀏覽器展示資料空間不足，請重設資料後再試");
  }

  await wait();
  return response(undefined);
};

const defaultAvatar = (id: number) => {
  const colors = ["#2F855A", "#2B6CB0", "#805AD5", "#C05621"];
  const color = colors[Math.abs(id) % colors.length];
  const svg = `
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 128 128">
      <rect width="128" height="128" rx="64" fill="${color}" />
      <circle cx="64" cy="48" r="24" fill="#FFFFFF" fill-opacity=".9" />
      <path d="M24 116c4-28 20-42 40-42s36 14 40 42" fill="#FFFFFF" fill-opacity=".9" />
    </svg>
  `;

  return `data:image/svg+xml,${encodeURIComponent(svg)}`;
};

export const employeeProfilePictureUrl = (id: number): string =>
  localStorage.getItem(`${IMAGE_STORAGE_PREFIX}${id}`) ?? defaultAvatar(id);

export const resetDemoData = () => {
  writeEmployees(structuredClone(seedEmployees));

  Object.keys(localStorage)
    .filter((key) => key.startsWith(IMAGE_STORAGE_PREFIX))
    .forEach((key) => localStorage.removeItem(key));
};
