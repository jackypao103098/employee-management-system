import {
    createContext,
    useContext,
    useEffect,
    useState
} from "react";
import {getEmployees, login as performLogin} from "../../services/client.js";
import jwtDecode from "jwt-decode";

const AuthContext = createContext({});

const AuthProvider = ({ children }) => {

    const [employee, setEmployee] = useState(null);

    const setEmployeeFromToken = () => {
        let token = localStorage.getItem("access_token");
        if (token) {
            try {
                token = jwtDecode(token);
                setEmployee({
                    username: token.sub,
                    roles: token.scopes
                })
            } catch (e) {
                localStorage.removeItem("access_token");
            }
        }
    }
    useEffect(() => {
        setEmployeeFromToken()
    }, [])


    const login = async (usernameAndPassword) => {
        return new Promise((resolve, reject) => {
            performLogin(usernameAndPassword).then(res => {
                const jwtToken = res.headers["authorization"];
                localStorage.setItem("access_token", jwtToken);

                const decodedToken = jwtDecode(jwtToken);

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

    const isEmployeeAuthenticated = () => {
        const token = localStorage.getItem("access_token");
        if (!token) {
            return false;
        }
        try {
            const { exp: expiration } = jwtDecode(token);
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
