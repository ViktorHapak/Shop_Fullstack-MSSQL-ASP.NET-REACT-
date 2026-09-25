import {memo, useCallback, useState} from "react";
import {useStateContext} from "../../_context/context_provider";
import {useHttp} from "../../_client/axios";
import styles from "../orders_area/Order.module.css";


function Order(props){

    const {token, role, authorities, setNotification} = useStateContext();

    const [focusId, setFocusId] = useState(props.focusId);
    const [order,setOrder] = useState(props.order);
    const [items, setItems] =  useState(props.order.items);

    const [focus, setFocus] = useState(false);

    const rejectOrder = useCallback(async (data) => {
        console.log(data);
        setNotification(`Rendelés elutasítva: \n Id: ${ data.id }`);
        props.cancelOrder();
    })

    const confirmOrder = useCallback(async (data) => {
        console.log(data);
        setNotification(`Rendelés elfogadva: \n Id: ${ data.id }`);
        props.cancelOrder();
    })

    const {sendRequest: rejectOrderRequest} = useHttp(`orders/reject=${props.order.id}`, null, 'POST', null, rejectOrder, true);
    const {sendRequest: confirmOrderRequest} = useHttp(`orders/confirm=${props.order.id}`, null, 'POST', null, confirmOrder, true);

    const formatStatus = (status) => {
        switch (status) {
            case 0:
                return "Pending";
            case 1:
                return "Waiting";
            case 2:
                return "Completed";
            case 3:
                return "Cancelled";
            default:
                return "Unknown";
        }
    };

    const formatDate = (dateValue) => {
        if (!dateValue) {return "-";}

        const date = new Date(dateValue);

        return date.toLocaleString("hu-HU", {
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit"
        });
    };

    const confirmOrderEnabled = () => {
        return (
            role === "Admin" ||
            (role === "Moderator" && authorities?.confirmOrder === true)
        );
    };

    const rejectOrderEnabled = () => {
        return (
            role === "Admin" ||
            (role === "Moderator" && authorities?.rejectOrder === true)
        );
    };

    return (
        <div className={styles["order-container"]}
             onMouseEnter={() => setFocus(true)}
             onMouseLeave={() => setFocus(false)}
        >
            {focus && order.status === 1 && (
                <div className={`${styles["operations-div"]}`}>
                    <button className={"btn btn-success btn-sm fw-bold"}
                            onClick={confirmOrderRequest}
                            disabled={!confirmOrderEnabled()}
                    >
                        Jóváhagyás
                    </button>
                    <button className={"btn btn-danger btn-sm fw-bold"}
                            onClick={rejectOrderRequest}
                            disabled={!rejectOrderEnabled()}
                    >
                        Elutasítás
                    </button>
                </div>
            )}
            <div className={styles["title-row"]}>
                <div className={styles["title-cell"]}>Id:</div>
                <div className={styles["value-cell"]}>{order.id}</div>
            </div>
            <div className={styles["user-row"]}>
                <div className={styles["title-cell"]}>Vásárló:</div>
                <div className={styles["value-cell"]}>{order.customerUsername}</div>
            </div>
            <div className={styles["status-row"]}>
                <div className={styles["title-cell"]}>Rendelés állapota:</div>
                <div className={styles["value-cell"]}>{formatStatus(order.status)}</div>
            </div>
            <div className={styles["created-row"]}>
                <div className={styles["title-cell"]}>Létrehozva:</div>
                <div className={styles["value-cell"]}>{formatDate(order.createdAt)}</div>
            </div>

            <div className={styles["submitted-row"]}>
                <div className={styles["title-cell"]}>Jóváhagyva:</div>
                <div className={styles["value-cell"]}>{formatDate(order.submittedAt)}</div>
            </div>
            <div className={styles["items-block"]}>
                <div className={styles["title-longcell"]}>Tételek:</div>
                <div className={styles["value-longblock"]}>
                    {order.items?.map((item) => {
                        const quantity = item.quantity > 1 ? ` ${item.quantity} db` : "";

                        const title = `${item?.productName}${quantity} :`;

                        return (
                            <div className={styles["item-row"]} key={item.id}>
                                <span className={styles["item-name"]}>
                                    {title}
                                </span>
                                <span className={styles["item-price"]}>
                                     {item?.totalPrice} Ft
                                </span>
                            </div>
                        );
                    })}
                </div>
            </div>
            <div className={styles["price-row"]}>
                <div className={styles["title-cell"]}>Összeg:</div>
                <div className={styles["value-cell"]}>{order?.totalPrice} Ft</div>
            </div>
        </div>
    )

}

export default memo(Order);