import axios from 'axios';

const getAuthConfig = () => ({
    headers: {
        Authorization: `Bearer ${localStorage.getItem("access_token")}`
    }
})

export const getEmployees = async () => {
    try {
        return await axios.get(
            `${import.meta.env.VITE_API_BASE_URL}/api/v1/employees`,
            getAuthConfig()
        )
    } catch (e) {
        throw e;
    }
}

export const saveEmployee = async (employee) => {
    try {
        return await axios.post(
            `${import.meta.env.VITE_API_BASE_URL}/api/v1/employees`,
            employee
        )
    } catch (e) {
        throw e;
    }
}

export const updateEmployee = async (id, update) => {
    try {
        return await axios.put(
            `${import.meta.env.VITE_API_BASE_URL}/api/v1/employees/${id}`,
            update,
            getAuthConfig()
        )
    } catch (e) {
        throw e;
    }
}

export const deleteEmployee = async (id) => {
    try {
        return await axios.delete(
            `${import.meta.env.VITE_API_BASE_URL}/api/v1/employees/${id}`,
            getAuthConfig()
        )
    } catch (e) {
        throw e;
    }
}

export const login = async (usernameAndPassword) => {
    try {
        return await axios.post(
            `${import.meta.env.VITE_API_BASE_URL}/api/v1/auth/login`,
            usernameAndPassword
        )
    } catch (e) {
        throw e;
    }
}

export const uploadEmployeeProfilePicture = async (id, formData) => {
    try {
        return await axios.post(
            `${import.meta.env.VITE_API_BASE_URL}/api/v1/employees/${id}/profile-image`,
            formData,
            {
                ...getAuthConfig(),
                "Content-Type": "multipart/form-data"
            }
        )
    } catch (e) {
        throw e;
    }
}

export const employeeProfilePictureUrl = id => {
    return `${import.meta.env.VITE_API_BASE_URL}/api/v1/employees/${id}/profile-image`
}
