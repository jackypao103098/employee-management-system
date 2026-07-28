import {
    Wrap,
    WrapItem,
    Spinner,
    Text
} from '@chakra-ui/react';
import SidebarWithHeader from "./components/shared/SideBar";
import { useEffect, useState } from 'react';
import { getEmployees, isDemoMode, resetDemoData } from "./services/client";
import CardWithImage from "./components/employee/EmployeeCard";
import CreateEmployeeDrawer from "./components/employee/CreateEmployeeDrawer";
import {errorNotification, successNotification} from "./services/notification";
import { Employee } from "./types/employee";
import DemoModeBanner from "./components/shared/DemoModeBanner";

const App = () => {

    const [employees, setEmployees] = useState<Employee[]>([]);
    const [loading, setLoading] = useState(false);
    const [err, setError] = useState("");

    const fetchEmployees = () => {
        setLoading(true);
        setError("");
        getEmployees().then(res => {
            setEmployees(res.data)
        }).catch(err => {
            const message = err.response?.data?.message ?? "Unable to load employees";
            setError(message)
            errorNotification(
                err.code ?? "EMPLOYEE_LOAD_ERROR",
                message
            )
        }).finally(() => {
            setLoading(false)
        })
    }

    const resetEmployees = () => {
        resetDemoData();
        fetchEmployees();
        successNotification("展示資料已重設", "員工資料已恢復為預設內容");
    }

    const demoModeBanner = isDemoMode
        ? <DemoModeBanner onReset={resetEmployees}/>
        : null;

    useEffect(() => {
        fetchEmployees();
    }, [])

    if (loading) {
        return (
            <SidebarWithHeader>
                {demoModeBanner}
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
                {demoModeBanner}
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
                {demoModeBanner}
                <CreateEmployeeDrawer
                    fetchEmployees={fetchEmployees}
                />
                <Text mt={5}>No employees available</Text>
            </SidebarWithHeader>
        )
    }

    return (
        <SidebarWithHeader>
            {demoModeBanner}
            <CreateEmployeeDrawer
                fetchEmployees={fetchEmployees}
            />
            <Wrap justify={"center"} spacing={"30px"}>
                {employees.map((employee, index) => (
                    <WrapItem key={employee.id}>
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
