import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';
import '@fortawesome/fontawesome-svg-core';
import '@fortawesome/react-fontawesome';
import {Outlet} from "react-router";
import styles from './Profile.module.css';

function Profile() {

    return (
        <div className={styles["modal-container"]}>
            <Outlet/>
        </div>
    )
}

export default Profile