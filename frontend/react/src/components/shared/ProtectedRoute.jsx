import {useEffect} from "react";
import {useNavigate} from "react-router-dom";
import {useAuth} from "../context/AuthContext.jsx";

const ProtectedRoute = ({ children }) => {

    const { isEmployeeAuthenticated } = useAuth()
    const navigate = useNavigate();

    useEffect(() => {
        if (!isEmployeeAuthenticated()) {
            navigate("/")
        }
    })

    return isEmployeeAuthenticated() ? children : "";
}

export default ProtectedRoute;