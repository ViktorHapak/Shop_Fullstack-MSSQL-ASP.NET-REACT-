import styles from './User.module.css';
import {memo, useCallback, useEffect, useState} from "react";
import {useHttp} from "../../_client/axios";
import {useStateContext} from "../../_context/context_provider";

function User(props) {

    const {error, errorMessage, errorState, loading,
        token, role, authorities,
        setError, setErrorMessage, setErrorState, setLoading, setNotification} = useStateContext();

    const [id, setId] = useState(0);
    const [user, setUser] = useState(props.user);
    const [ownuser, setOwnUser] = useState(props.ownuser);

    const getUser = useCallback(async (data) => {
        setUser(data);
    }, []);


    const addVisitor = useCallback(async (data) => {
        setNotification(`Felhasználói jogosultság: \n${data.user}`);
        await getUserRequest()
    }, []);

    const addModerator = useCallback(async (data) => {
        setNotification(`Moderátori jogosultság: \n${data.user}`);
        await getUserRequest()
    }, []);

    const addAdmin = useCallback(async (data) => {
        setNotification(`Admin jogosultság: \n${data.user}`);
        await getUserRequest()
    }, []);

    const {sendRequest: getUserRequest} = useHttp(`users/${user?.id}`, null, 'GET', null, getUser);
    const {sendRequest: addVisitorRoleRequest} = useHttp( `users/${user.id}`, {role: 'Visitor'}, 'PUT', null, addVisitor);
    const {sendRequest: addModeratorRoleRequest} = useHttp( `users/${user.id}`, {role: 'Moderator'}, 'PUT', null, addModerator);
    const {sendRequest: addAdminRoleRequest} = useHttp( `users/${user.id}`, {role: 'Admin'}, 'PUT', null, addAdmin);

    useEffect(() => {
        setUser(props.user);
        setId(props.focusId);
    }, [props.user]);

    const addRoleEnabled = (_username, _role) => {
        return (
            ownuser.role === "Admin" && (_username !== (ownuser?.username ?? "")) && (_role !== convertToRoleName(user.role))
        );
    };

    const convertToRoleName = (role) => {
        switch (role) {
            case 1: return "Moderator"; break;
            case 2: return "Admin"; break;
            default: return "Visitor"; break;
        }
    }

    const formatDate = (date) => {
        if (!date) return "";
        return new Date(date).toISOString().split("T")[0];
    };

    return (
        <div className={`${styles["user-container"]}`}>
            <div className={`${styles["row"]}`}>
                <a align={"left"} className={styles["key-cell"]}>Felhasználónév:</a>
                <a align={"right"} className={styles["value-cell"]}> {user.username || ""}</a>
            </div>
            <div className={`${styles["row"]}`}>
                <a align={"left"} className={styles["key-cell"]}>Email:</a>
                <a align={"right"} className={styles["value-cell"]}> {user.email || ""}</a>
            </div>
            <div className={`${styles["row"]}`}>
                <a align={"left"} className={styles["key-cell"]}>Szül. dátum:</a>
                <a align={"right"} className={styles["value-cell"]}> {formatDate(user.birth) || ""}</a>
            </div>
            <div className={`${styles["row"]}`}>
                <a align={"left"} className={styles["key-cell"]}>Jogosultság:</a>
                <a align={"right"} className={styles["value-cell"]}> {convertToRoleName(user.role)}</a>
            </div>
            <div className={`${styles["long-row"]}`}>
                <a align={"left"} className={styles["key-longcell"]}>Kódolt jelszó:</a>
                <a align={"right"} className={styles["value-longcell"]}> {user.password || ""}</a>
            </div>
            <div className={`${styles["operations-container"]}`}>
                {addRoleEnabled(user.username, "Visitor") && (
                    <button className="btn btn-primary btn-sm fw-bold"
                            onClick={() => addVisitorRoleRequest()}
                    >
                        <i className="fa fa-plus"></i> Látogató
                    </button>
                )}
                {addRoleEnabled(user.username, "Moderator") && (
                    <button type="button"
                            className="btn btn-primary btn-sm fw-bold"
                            onClick={() => addModeratorRoleRequest()}
                    >
                        <i className="fa fa-plus"></i> Moderátor
                    </button>
                )}
                {addRoleEnabled(user.username, "Admin") && (
                    <button type="button"
                            className="btn btn-primary btn-sm fw-bold"
                            onClick={() => addAdminRoleRequest()}
                    >
                        <i className="fa fa-plus"></i> Admin
                    </button>
                )}
            </div>
        </div>
    )


}

export default memo(User)