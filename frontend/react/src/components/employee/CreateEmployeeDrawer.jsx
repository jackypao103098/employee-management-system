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
import CreateEmployeeForm from "../shared/CreateEmployeeForm.jsx";

const AddIcon = () => "+";
const CloseIcon = () => "x";

const CreateEmployeeDrawer = ({ fetchEmployees }) => {
    const { isOpen, onOpen, onClose } = useDisclosure()
    return <>
        <Button
            leftIcon={<AddIcon/>}
            colorScheme={"green"}
            onClick={onOpen}
        >
            Create Employee
        </Button>
        <Drawer isOpen={isOpen} onClose={onClose} size={"xl"}>
            <DrawerOverlay />
            <DrawerContent>
                <DrawerCloseButton />
                <DrawerHeader>Create new employee</DrawerHeader>

                <DrawerBody>
                    <CreateEmployeeForm
                        onSuccess={fetchEmployees}
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

export default CreateEmployeeDrawer;
