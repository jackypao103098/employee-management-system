import {
    Button,
    Drawer,
    DrawerBody,
    DrawerCloseButton,
    DrawerContent,
    DrawerFooter,
    DrawerHeader,
    DrawerOverlay,
    useDisclosure
} from "@chakra-ui/react";
import UpdateEmployeeForm from "./UpdateEmployeeForm.jsx";

const CloseIcon = () => "x";

const UpdateEmployeeDrawer = ({ fetchEmployees, initialValues, employeeId }) => {
    const { isOpen, onOpen, onClose } = useDisclosure()
    return <>
        <Button
            bg={'gray.200'}
            color={'black'}
            rounded={'full'}
            _hover={{
                transform: 'translateY(-2px)',
                boxShadow: 'lg'
            }}
            onClick={onOpen}
        >
            Update
        </Button>
        <Drawer isOpen={isOpen} onClose={onClose} size={"xl"}>
            <DrawerOverlay />
            <DrawerContent>
                <DrawerCloseButton />
                <DrawerHeader>Update employee</DrawerHeader>

                <DrawerBody>
                    <UpdateEmployeeForm
                        fetchEmployees={fetchEmployees}
                        initialValues={initialValues}
                        employeeId={employeeId}
                    />
                </DrawerBody>

                <DrawerFooter>
                    <Button
                        leftIcon={<CloseIcon/>}
                        colorScheme={"green"}
                        onClick={onClose}>
                    Close
                    </Button>
                </DrawerFooter>
            </DrawerContent>
        </Drawer>
        </>

}

export default UpdateEmployeeDrawer;
