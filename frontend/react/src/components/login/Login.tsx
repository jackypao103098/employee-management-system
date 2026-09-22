import {
    Alert,
    AlertIcon,
    Box,
    Button,
    Code,
    Flex,
    FormLabel,
    Heading,
    Image,
    Input,
    Stack,
    Text,
} from '@chakra-ui/react';
import {Formik, Form, useField} from "formik";
import * as Yup from 'yup';
import {useAuth} from "../context/AuthContext";
import {errorNotification} from "../../services/notification";
import {useNavigate} from "react-router-dom";
import {useEffect} from "react";
import {isDemoMode} from "../../services/client";
import {getApiErrorMessage} from "../../services/apiError";

const MyTextInput = ({label, ...props}: { label: string; name: string; [key: string]: any }) => {
    const [field, meta] = useField(props);
    return (
        <Box>
            <FormLabel htmlFor={props.id || props.name}>{label}</FormLabel>
            <Input className="text-input" {...field} {...props} />
            {meta.touched && meta.error ? (
                <Alert className="error" status={"error"} mt={2}>
                    <AlertIcon/>
                    {meta.error}
                </Alert>
            ) : null}
        </Box>
    );
};

const LoginForm = () => {
    const { login } = useAuth();
    const navigate = useNavigate();

    return (
        <Formik
            validateOnMount={true}
            validationSchema={
                Yup.object({
                    email: Yup.string()
                        .email("Must be valid email")
                        .required("Email is required"),
                    password: Yup.string()
                        .min(8, "Password must be at least 8 characters")
                        .max(100, "Password cannot be more than 100 characters")
                        .required("Password is required")
                })
            }
            initialValues={{
                email: isDemoMode ? 'demo@jackypao.com' : '',
                password: isDemoMode ? 'password' : ''
            }}
            onSubmit={(values, {setSubmitting}) => {
                setSubmitting(true);
                login(values).then(() => {
                    navigate("/dashboard")
                }).catch(err => {
                    errorNotification(
                        err.code ?? "LOGIN_FAILED",
                        getApiErrorMessage(err, "Email or password is incorrect.")
                    )
                }).finally(() => {
                    setSubmitting(false);
                })
            }}>

            {({isValid, isSubmitting}) => (
                <Form>
                    <Stack mt={15} spacing={15}>
                        <MyTextInput
                            label={"Email"}
                            name={"email"}
                            type={"email"}
                            placeholder={"hello@example.com"}
                        />
                        <MyTextInput
                            label={"Password"}
                            name={"password"}
                            type={"password"}
                            placeholder={"Type your password"}
                        />

                        <Button
                            type={"submit"}
                            disabled={!isValid || isSubmitting}>
                            Login
                        </Button>
                    </Stack>
                </Form>
            )}

        </Formik>
    )
}

const Login = () => {

    const { employee } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        if (employee) {
            navigate("/dashboard");
        }
    }, [employee, navigate])

    return (
        <Stack minH={'100vh'} direction={{base: 'column', md: 'row'}}>
            <Flex p={8} flex={1} alignItems={'center'} justifyContent={'center'}>
                <Stack spacing={4} w={'full'} maxW={'md'}>
                    <Heading fontSize={'2xl'} mb={15}>Employee Management System</Heading>
                    <Text color={'gray.500'} mb={5}>Sign in to manage your employees</Text>
                    {isDemoMode && (
                        <Box bg={'green.50'} border={'1px'} borderColor={'green.200'} borderRadius={'md'} p={4}>
                            <Text fontWeight={'bold'} color={'green.700'} mb={2}>展示模式</Text>
                            <Text fontSize={'sm'} mb={2}>
                                資料只會儲存在目前瀏覽器，不會連線到正式後端。
                            </Text>
                            <Text fontSize={'sm'}>Email: <Code colorScheme='green'>demo@jackypao.com</Code></Text>
                            <Text fontSize={'sm'}>Password: <Code colorScheme='green'>password</Code></Text>
                        </Box>
                    )}
                    <LoginForm/>
                </Stack>
            </Flex>
            <Flex
                flex={1}
                p={10}
                flexDirection={"column"}
                alignItems={"center"}
                justifyContent={"center"}
                bgGradient={{sm: 'linear(to-r, green.600, teal.600)'}}
            >
                <Text fontSize={"6xl"} color={'white'} fontWeight={"bold"} mb={5}>
                    Employee Management
                </Text>
                <Image
                    alt={'Login Image'}
                    objectFit={'scale-down'}
                    src={
                        'https://user-images.githubusercontent.com/40702606/215539167-d7006790-b880-4929-83fb-c43fa74f429e.png'
                    }
                />
            </Flex>
        </Stack>
    );
}

export default Login;
