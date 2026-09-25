import styles from './CleanDepartmentForm.module.css';
import {useHttp} from "../../_client/axios";
import {useStateContext} from "../../_context/context_provider";
import {useCallback, useEffect} from "react";

function CleanDepartmentForm(props) {

    const {products,
        error, errorMessage, errorState, loading,
        role, authorities, setNotification} = useStateContext();


    const cleanStock = useCallback(async data => {
        setNotification(data);
        await props.getProducts();
        props.closeForm();
    }, []);

    const cleanType = useCallback(async (data) => {
        setNotification(data);
        await props.getProducts();
        props.closeForm();
    }, []);


    const {sendRequest: cleanStockRequest} = useHttp(`products/dep/clean`, {mode: 'stock', departmentId: props.id}, 'PUT', null, cleanStock);
    const {sendRequest: cleanTypeRequest} = useHttp(`products/dep/clean`, {mode: 'type', departmentId: props.id}, 'PUT', null, cleanType);

    useEffect(() => {
        console.log(props.id);
    }, [])

    const replenishStackEnabled = () => {
        return (
            role === "Admin" ||
            (role === "Moderator" && authorities?.replenishStack === true)
        );
    };


    return (
        <div className={`${styles["clean-container"]}`}>
            <div className={`${styles["title-container"]}`}>
                <h6 className={`${styles["title"]}`}>Részleg tisztítása</h6>
                <button className={`${styles["close-item"]}`} onClick={() => props.closeForm()}>
                    <i className="fa fa-close"></i>
                </button>
            </div>
            <hr/>
            <div className={`${styles["options-container"]}`}>
                <button className={`${styles["stock-clean"]} btn btn-primary btn-sm fw-bold`}
                        title="A részleg teljes készletének eltávolítása"
                        disabled={!replenishStackEnabled()}
                        onClick={() => {cleanStockRequest();}}
                >
                    Készlet
                </button>

                <button className={`${styles["type-clean"]} btn btn-danger btn-sm fw-bold`}
                        title="A részleg teljes készletének eltávolítása"
                        disabled={!replenishStackEnabled()}
                        onClick={() => {cleanTypeRequest();}}
                >
                    Termékek
                </button>
            </div>
        </div>
    )
}

export default CleanDepartmentForm;