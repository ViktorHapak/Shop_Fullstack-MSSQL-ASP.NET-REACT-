import {memo, useCallback, useState} from "react";
import {useStateContext} from "../../_context/context_provider";
import {useHttp} from "../../_client/axios";
import styles from "../orders/OwnOrder.module.css";


function CartObject({cart}) {

    const {token, loading, error, errorMessage, errorState,
        setLoading, setError, setErrorMessage, setErrorState, setNotification} = useStateContext();


    const [focus, setFocus] = useState(false);
    const [items, setItems] =  useState(cart.items);

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
            <div className={styles["title-row"]}>
                <div className={styles["title-cell"]}>Id:</div>
                <div className={styles["value-cell"]}>{cart.id}</div>
            </div>
            <div className={styles["user-row"]}>
                <div className={styles["title-cell"]}>Vásárló:</div>
                <div className={styles["value-cell"]}>{cart.user?.username}</div>
            </div>
            <div className={styles["created-row"]}>
                <div className={styles["title-cell"]}>Létrehozva:</div>
                <div className={styles["value-cell"]}>{formatDate(cart.createdAt)}</div>
            </div>
            <div className={styles["items-block"]}>
                <div className={styles["title-longcell"]}>Tételek:</div>
                <div className={styles["value-longblock"]}>
                    {cart.items?.map((item) => {
                        const quantity = item.quantity > 1 ? ` ${item.quantity} db` : "";

                        const title = `${item.product?.name}${quantity} :`;

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
                <div className={styles["value-cell"]}>{cart?.totalPrice} Ft</div>
            </div>
        </div>
    )

}

export default memo(CartObject);