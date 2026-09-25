import {createBrowserRouter, Navigate} from "react-router";
import Main from "../components/Main";
import CartContent from "../components/cart/CartContent";
import Products from "../components/shopping_area/Products";
import OwnOrders from "../components/orders/OwnOrders";
import Users from "../components/users/Users";
import Profile from "../components/profile/Profile";
import Register from "../components/profile/register/Register";
import Login from "../components/profile/login/Login";
import ProfileInfo from "../components/profile/info/ProfileInfo";
import AdminPanel from "../components/admin/AdminPanel";
import Carts from "../components/carts_area/Carts";
import Orders from "../components/orders_area/Orders";
import Others from "../components/others_area/Others";


export const Routes = createBrowserRouter([
    { path: '/', index:true, element: <Navigate to="/main" replace/> },
    {
        path: '/main',
        element: <Main/>,
        children: [
            {
                index: true, element: <Navigate to="shopping-area" replace/>
            },
            {
                path: 'shopping-area',
                element: <Products/>
            },
            {
                path: 'cart_content',
                element: <CartContent/>,
            },
            {
                path: 'orders',
                element: <OwnOrders/>,
            },
            {
                path: 'admin_panel',
                element: <AdminPanel/>,
            },
            {
                path: 'carts_area',
                element: <Carts/>,
            },
            {
                path: 'orders_area',
                element: <Orders/>,
            },
            {
                path: 'users',
                element: <Users/>,
            },
            {
                path: 'others_area',
                element: <Others/>
            },
            {
                path: 'profile',
                element: <Profile/>,
                children: [
                    {
                        index: true, element: <Navigate to="info" replace/>
                    },
                    {
                        path: 'signup',
                        element: <Register/>,
                    },
                    {
                        path: 'login',
                        element: <Login/>
                    },
                    {
                        path: 'info',
                        element: <ProfileInfo/>
                    }
                ]
            }
        ]
    }
])