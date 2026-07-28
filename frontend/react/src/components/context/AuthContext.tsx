import {
    createContext,
    ReactNode,
    useContext,
    useEffect,
    useState
} from "react";
import { AxiosResponse } from "axios";
import { login as performLogin } from "../../services/client";
import jwtDecode from "jwt-decode";
import { AuthenticatedEmployee, JwtToken, LoginRequest } from "../../types/employee";

interface AuthContextType {
    employee: AuthenticatedEmployee | null;
    login: (usernameAndPassword: LoginRequest) => Promise<AxiosResponse>;
    logOut: () => void;
    isEmployeeAuthenticated: () => boolean;
    setEmployeeFromToken: () => void;
}

const AuthContext = createContext<AuthContextType>({} as AuthContextType);

const AuthProvider = ({ children }: { children: ReactNode }) => {

    const [employee, setEmployee] = useState<AuthenticatedEmployee | null>(null);

    const setEmployeeFromToken = () => {
        const token = localStorage.getItem("access_token");
        if (token) {
            try {
                const decodedToken = jwtDecode<JwtToken>(token);
                setEmployee({
                    username: decodedToken.sub,
                    roles: decodedToken.scopes
                })
            } catch (e) {
                localStorage.removeItem("access_token");
            }
        }
    }
    useEffect(() => {
        setEmployeeFromToken()
    }, [])


    const login = async (usernameAndPassword: LoginRequest): Promise<AxiosResponse> => {
        return new Promise((resolve, reject) => {
            performLogin(usernameAndPassword).then(res => {
                const jwtToken = res.headers["authorization"] as string;
                localStorage.setItem("access_token", jwtToken);

                const decodedToken = jwtDecode<JwtToken>(jwtToken);

                setEmployee({
                    username: decodedToken.sub,
                    roles: decodedToken.scopes
                })
                resolve(res);
            }).catch(err => {
                reject(err);
            })
        })
    }

    const logOut = () => {
        localStorage.removeItem("access_token")
        setEmployee(null)
    }

    const isEmployeeAuthenticated = (): boolean => {
        const token = localStorage.getItem("access_token");
        if (!token) {
            return false;
        }
        try {
            const { exp: expiration } = jwtDecode<JwtToken>(token);
            if (Date.now() > expiration * 1000) {
                logOut()
                return false;
            }
            return true;
        } catch (e) {
            logOut();
            return false;
        }
    }

    return (
        <AuthContext.Provider value={{
            employee,
            login,
            logOut,
            isEmployeeAuthenticated,
            setEmployeeFromToken
        }}>
            {children}
        </AuthContext.Provider>
    )
}

export const useAuth = () => useContext(AuthContext);

export default AuthProvider;
