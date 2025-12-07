import { createPortal } from "react-dom";
import NotificationBar from "./NotificationBar";

interface NotificationBarPortalProps {
  message: string;
  isVisible: boolean;
  type: "success" | "error";
  onClose?: () => void;
  autoDismiss?: boolean;
  dismissTime?: number;
}

const NotificationBarPortal: React.FC<NotificationBarPortalProps> = (props) => {
  return createPortal(
    <NotificationBar {...props} />,
    document.body
  );
};

export default NotificationBarPortal;
