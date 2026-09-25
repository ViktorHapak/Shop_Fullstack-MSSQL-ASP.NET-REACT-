import {useStateContext} from "../../_context/context_provider";
import styles from "./OwnOrder.module.css";
import {memo, useCallback, useState} from "react";
import orders from "./OwnOrders";
import {useHttp} from "../../_client/axios";


function OwnOrder(props){


    const {token, loading, error, errorMessage, errorState,
        setLoading, setError, setErrorMessage, setErrorState, setNotification} = useStateContext();
    const [focus, setFocus] = useState(false);

    const [edit, setEdit] = useState(props.edit);
    const [order,setOrder] = useState(props.order);
    const [items, setItems] =  useState(props.order.items);

    const cancelOrder = useCallback(async (data) => {
        console.log(data);
        setNotification(`Rendelés visszavonva: \n Id: ${ props.order.id }`);
        props.cancelOrder();
    })

    const {sendRequest: cancelOrderRequest} = useHttp(`orders/cancel=${order.id}`, null, 'POST', null, cancelOrder, true);

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

    return (
        <div className={styles["order-container"]}
                onMouseEnter={() => setFocus(true)}
                onMouseLeave={() => setFocus(false)}
        >
            {focus && order.status === 1 && (
                <button
                    type="button"
                    className={`btn btn-danger ${styles["close-btn"]}`}

                    onClick={() => cancelOrderRequest()}
                >
                    X
                </button>
            ) /*We can cancel an order, if it in waiting status*/}
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
                    {items.map((item) => {
                        const quantity = item.quantity > 1 ? ` ${item.quantity} db` : "";

                        const title = `${item.productName}${quantity} :`;

                        return (
                            <div className={styles["item-row"]} key={item.id}>
                                <span className={styles["item-name"]}>
                                    {title}
                                </span>
                                <span className={styles["item-price"]}>
                                     {item.totalPrice} Ft
                                </span>
                            </div>
                        );
                    })}
                </div>
            </div>
            <div className={styles["price-row"]}>
                <div className={styles["title-cell"]}>Összeg:</div>
                <div className={styles["value-cell"]}>{order.totalPrice} Ft</div>
            </div>
        </div>
    )

}

export default memo(OwnOrder);