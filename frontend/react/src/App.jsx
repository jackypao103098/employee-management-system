import {
    Wrap,
    WrapItem,
    Spinner,
    Text
} from '@chakra-ui/react';
import SidebarWithHeader from "./components/shared/SideBar.jsx";
import { useEffect, useState } from 'react';
import { getEmployees } from "./services/client.js";
import CardWithImage from "./components/employee/EmployeeCard.jsx";
import CreateEmployeeDrawer from "./components/employee/CreateEmployeeDrawer.jsx";
import {errorNotification} from "./services/notification.js";

const App = () => {

    const [employees, setEmployees] = useState([]);
    const [loading, setLoading] = useState(false);
    const [err, setError] = useState("");

    const fetchEmployees = () => {
        setLoading(true);
        getEmployees().then(res => {
            setEmployees(res.data)
        }).catch(err => {
            setError(err.response.data.message)
            errorNotification(
                err.code,
                err.response.data.message
            )
        }).finally(() => {
            setLoading(false)
        })
    }

    useEffect(() => {
        fetchEmployees();
    }, [])

    if (loading) {
        return (
            <SidebarWithHeader>
                <Spinner
                    thickness='4px'
                    speed='0.65s'
                    emptyColor='gray.200'
                    color='green.500'
                    size='xl'
                />
            </SidebarWithHeader>
        )
    }

    if (err) {
        return (
            <SidebarWithHeader>
                <CreateEmployeeDrawer
                    fetchEmployees={fetchEmployees}
                />
                <Text mt={5}>Ooops there was an error</Text>
            </SidebarWithHeader>
        )
    }

    if(employees.length <= 0) {
        return (
            <SidebarWithHeader>
                <CreateEmployeeDrawer
                    fetchEmployees={fetchEmployees}
                />
                <Text mt={5}>No employees available</Text>
            </SidebarWithHeader>
        )
    }

    return (
        <SidebarWithHeader>
            <CreateEmployeeDrawer
                fetchEmployees={fetchEmployees}
            />
            <Wrap justify={"center"} spacing={"30px"}>
                {employees.map((employee, index) => (
                    <WrapItem key={index}>
                        <CardWithImage
                            {...employee}
                            imageNumber={index}
                            fetchEmployees={fetchEmployees}
                        />
                    </WrapItem>
                ))}
            </Wrap>
        </SidebarWithHeader>
    )
}

export default App;
