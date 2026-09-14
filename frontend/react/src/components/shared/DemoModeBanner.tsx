import {
  Alert,
  AlertDescription,
  AlertIcon,
  AlertTitle,
  Button,
  Flex,
} from "@chakra-ui/react";

interface DemoModeBannerProps {
  onReset: () => void;
}

const DemoModeBanner = ({ onReset }: DemoModeBannerProps) => (
  <Alert
    status="info"
    variant="left-accent"
    borderRadius="md"
    mb={6}
    alignItems={{ base: "flex-start", md: "center" }}
  >
    <AlertIcon mt={{ base: 1, md: 0 }} />
    <Flex
      flex="1"
      gap={{ base: 2, md: 4 }}
      direction={{ base: "column", md: "row" }}
      align={{ base: "flex-start", md: "center" }}
      justify="space-between"
    >
      <div>
        <AlertTitle>展示模式</AlertTitle>
        <AlertDescription>
          所有異動只會儲存在這個瀏覽器，不會寫入正式資料庫。
        </AlertDescription>
      </div>
      <Button size="sm" colorScheme="blue" variant="outline" onClick={onReset}>
        重設展示資料
      </Button>
    </Flex>
  </Alert>
);

export default DemoModeBanner;
