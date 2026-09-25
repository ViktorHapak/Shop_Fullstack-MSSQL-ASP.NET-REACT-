import {createContext, useCallback, useContext, useEffect, useState} from "react";
import {Product} from "../_models/Product";
import {useHttp} from "../_client/axios";
import productImageUrl from "../assets/product.webp";
import {useNavigate} from "react-router";

const StateContext = createContext({
    user: null,
    token: null,
    authorities: null,
    notification: null,
    role: null,
    editMode: null,
    error: null,
    errorMessage: null,
    errorState: null,
    loading: null,
    products: null,
    categories: null,
    minPrice: null,
    maxPrice: null,
    productImgs: null,
    pages: null,
    parameters: null,
    setUser: () => {},
    setToken: () => {},
    setAuthorities: () => {},
    setNotification: () => {},
    setRole: () => {},
    setEditMode: () => {},
    setError: () => {},
    setErrorMessage: () => {},
    setErrorState: () => {},
    setLoading: () => {},
    setProducts: () => {},
    setCategories: () => {},
    setMinPrice: () => {},
    setMaxPrice: () => {},
    setProductImgs: () => {},
    setPages: () => {},
    setParameters: () => {},
});

export const ContextProvider  = ({children}) => {
    const [user, setUser] = useState({ name: null });
    const [notification, _setNotification] = useState('')
    const [token, _setToken] = useState(localStorage.getItem('ACCESS_TOKEN'));
    const [authorities, setAuthorities] = useState({});
    const [role, _setRole] = useState(localStorage.getItem('ROLE'));

    const [editMode, _setEditMode] = useState(false);
    const [error, setError] = useState(null);
    const [errorMessage, setErrorMessage] = useState("");
    const [loading, setLoading] = useState(false);
    const [errorState, setErrorState] = useState(null);

    const [products, setProducts] = useState([]);
    const [categories, setCategories] = useState([]);
    const [minPrice, setMinPrice] = useState(0.0);
    const [maxPrice, setMaxPrice] = useState(0.0);
    const [productImgs, setProductImgs] = useState(null);
    const [pages, setPages] = useState(0);
    const [parameters, _setParameters] =
        useState({page : 0, size : 8, title: "", department_name: null, min_price: null, max_price: null, order: "default"});

    useEffect(() => {
        if (minPrice == 0 || minPrice == null)
            setParameters({...parameters, minPrice: null})
    }, [minPrice]);

    useEffect(() => {
        if (maxPrice == 0 || maxPrice == null)
            setParameters({...parameters, maxPrice: null})

    }, [maxPrice]);

    const setNotification = (message) => {
        _setNotification(message);
        setTimeout(() => {
            _setNotification('')
        }, 5000)
    }

    const setToken = (token) => {
        console.log("SET TOKEN RECEIVED:", token);
        _setToken(token)
        localStorage.setItem('ACCESS_TOKEN', token)
    }

    const setRole = (role) => {
        _setRole(role);
        localStorage.setItem('ROLE', role)
    }

    const removeToken = () => {
        _setToken(null);
        _setRole(null);
        setError({});
        localStorage.removeItem('ACCESS_TOKEN');
        localStorage.removeItem('ROLE')

        console.log("Removed token!")
    }

    const setEditMode = () => {
        setEditMode(!editMode);
    }

    const setParameters = (parameters) => {
        _setParameters(parameters);
    }

    return (
        // eslint-disable-next-line react/jsx-no-undef
        <StateContext.Provider value={{
            user, setUser,
            token, setToken, removeToken,
            notification, setNotification,
            role, setRole,
            authorities, setAuthorities,
            editMode, setEditMode,
            error, setError,
            errorMessage, setErrorMessage,
            errorState, setErrorState,
            loading, setLoading,
            products, setProducts,
            categories, setCategories,
            minPrice, setMinPrice,
            maxPrice, setMaxPrice,
            productImgs, setProductImgs,
            pages, setPages,
            parameters, setParameters
        }}>
            {children}
        </StateContext.Provider>
    );
}

export const useStateContext = () => useContext(StateContext);