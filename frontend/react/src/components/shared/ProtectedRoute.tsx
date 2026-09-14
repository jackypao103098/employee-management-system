import {useEffect} from "react";
import {useNavigate} from "react-router-dom";
import {useAuth} from "../context/AuthContext";
import {isAuthenticationEnabled} from "../../services/client";

const ProtectedRoute = ({ children }) => {

    const { employee } = useAuth()
    const navigate = useNavigate();

    useEffect(() => {
        if (isAuthenticationEnabled && !employee) {
            navigate("/")
        }
    }, [employee, navigate])

    return !isAuthenticationEnabled || employee ? children : null;
}

export default ProtectedRoute;
