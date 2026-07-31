import * as demoClient from "./demoClient";
import * as realClient from "./realClient";

export const isDemoMode = import.meta.env.VITE_API_MODE === "demo";

const activeClient = isDemoMode ? demoClient : realClient;

export const getEmployees = activeClient.getEmployees;
export const saveEmployee = activeClient.saveEmployee;
export const updateEmployee = activeClient.updateEmployee;
export const deleteEmployee = activeClient.deleteEmployee;
export const login = activeClient.login;
export const uploadEmployeeProfilePicture =
  activeClient.uploadEmployeeProfilePicture;
export const employeeProfilePictureUrl =
  activeClient.employeeProfilePictureUrl;

export const resetDemoData = () => {
  if (isDemoMode) {
    demoClient.resetDemoData();
  }
};
