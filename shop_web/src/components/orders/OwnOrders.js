import {useCallback, useEffect, useState} from "react";
import styles from "./OwnOrders.module.css";
import {useStateContext} from "../../_context/context_provider";
import {useHttp} from "../../_client/axios";
import Order from "./OwnOrder";

function OwnOrders() {
    const {token, loading, error, errorMessage, errorState,
        setLoading, setError, setErrorMessage, setErrorState, setNotification} = useStateContext();
    const [orders, setOrders] = useState([]);
    const [status, setStatus] = useState("Waiting")
    const [edit, setEdit] = useState(null);

    const states = ["Pending","Waiting","Completed","Cancelled"]

    const getOrders = useCallback(async (data) => {
        setOrders(data);
    })

    const {sendRequest: getOrdersRequest} = useHttp("orders/own", {status: status}, 'GET', null, getOrders);


    useEffect(() => {
        getOrdersRequest();
    }, [status])

    useEffect(() => {
        return () => {
            setError(null);
            setLoading(false);
            setErrorMessage('');
        };
    }, [setError, setLoading, setErrorMessage]);

    const setEditIndex = (index) => {setEdit(index);};

    return (
         <div className={`${styles["orders-container"]}`}>
             <div className={styles["title-div"]}>
                 <h2 className={styles["title"]}>Rendelések:</h2>
             </div>
             <div className={styles["status-div"]}>
                 {states.map((value) => {
                     const id = `status-${value}`;
                     return (
                         <div className={styles["radio-div"]} key={value}>
                             <label htmlFor={id}>
                                 {value}:
                             </label>

                             <input
                                 id={id}
                                 name="order-status"
                                 type="radio"
                                 value={value}
                                 checked={status === value}
                                 onChange={(e) => setStatus(e.target.value)}
                             />
                         </div>
                     );
                 })}
             </div>
             {(loading || !orders) && !error &&
                 (<div className={`${styles["loading-container"]}`}>
                            <span className={`${styles["loading-title"]}`}>
                                <label>Loading</label>
                                <label className={`${styles["dot-first"]}`}>.</label>
                                <label className={`${styles["dot-second"]}`}>.</label>
                                <label className={`${styles["dot-third"]}`}>.</label>
                            </span>
                 </div>)
             }
             {!loading && !error && orders &&
                 (
                     <div className={`${styles["orders-div"]}`}>
                         {orders.map((order, index) => (
                             <Order key={index} order={order} edit={edit} cancelOrder={getOrdersRequest} setEditIndex={setEditIndex} />
                         ))}
                     </div>
                 )
             }
             {!loading && !error && orders?.length === 0 && (
                 <div className={styles["empty-message"]}>
                     Üres rendeléslista!
                 </div>
             )}
             {!loading && error && (errorMessage.length > 2) && (
                 (errorState === 500) ?
                     (
                         <div className="alert alert-warning d-block mx-5 m-3">
                             Szerver hiba!
                         </div>
                     ) : (
                         <div className="alert alert-danger d-block mx-5 m-3">
                             {errorMessage}
                         </div>
                     )
             )
             }

         </div>
    )
}

export default OwnOrders