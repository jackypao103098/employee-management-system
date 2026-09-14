import {createContext, ReactNode, useContext, useState} from "react";
import {login as performLogin} from "../../services/client";
import jwtDecode from "jwt-decode";
import {AuthenticatedEmployee, JwtToken, LoginRequest} from "../../types/employee";
import {
    clearAccessToken,
    getAccessToken,
    setAccessToken
} from "../../services/authToken";

interface AuthContextType {
    employee: AuthenticatedEmployee | null;
    login: (credentials: LoginRequest) => Promise<void>;
    logOut: () => void;
    isAdmin: boolean;
    canManageEmployee: (employeeId: number) => boolean;
}

const AuthContext = createContext<AuthContextType>({} as AuthContextType);

const readEmployeeFromToken = (): AuthenticatedEmployee | null => {
    const token = getAccessToken();
    if (!token) {
        return null;
    }

    try {
        const claims = jwtDecode<JwtToken>(token);
        const employeeId = Number(claims.sub);
        if (
            !Number.isInteger(employeeId) ||
            !claims.email ||
            !["ADMIN", "EMPLOYEE"].includes(claims.role) ||
            Date.now() >= claims.exp * 1000
        ) {
            clearAccessToken();
            return null;
        }

        return {
            id: employeeId,
            email: claims.email,
            role: claims.role
        };
    } catch {
        clearAccessToken();
        return null;
    }
};

const AuthProvider = ({children}: { children: ReactNode }) => {
    const [employee, setEmployee] = useState<AuthenticatedEmployee | null>(
        readEmployeeFromToken
    );

    const login = async (credentials: LoginRequest): Promise<void> => {
        const response = await performLogin(credentials);
        setAccessToken(response.data.accessToken);
        setEmployee(readEmployeeFromToken());
    };

    const logOut = () => {
        clearAccessToken();
        setEmployee(null);
    };

    const isAdmin = employee?.role === "ADMIN";
    const canManageEmployee = (employeeId: number): boolean =>
        isAdmin || employee?.id === employeeId;

    return (
        <AuthContext.Provider value={{
            employee,
            login,
            logOut,
            isAdmin,
            canManageEmployee
        }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);

export default AuthProvider;
