import {useCallback, useContext, useState} from "react";
import axios from "axios";
import {useStateContext} from "../_context/context_provider";
import * as headers from "react-hook-form";


const httpClient = axios.create({
    baseURL: "http://localhost:5167/api"
})

httpClient.interceptors.request.use((config) => {
    const token = localStorage.getItem('ACCESS_TOKEN');

    if (token){
        config.headers.set('Authorization', `Bearer ${token}`);
    }

    return config;
})

export function useHttp(url, queries, method = 'GET', body = null, action = null, freeze = false) {
    const {error, errorMessage, errorState, loading,
        setError, setErrorMessage, setErrorState, setLoading} = useStateContext();

    const sendRequest = useCallback(
        async () => {
            setLoading(true);

            const token = localStorage.getItem('ACCESS_TOKEN');
            const params = new URLSearchParams();
            try {
                if (queries) {
                    Object.entries(queries).forEach(([key, value]) => {

                        if (value === null || value === undefined) {
                            return;
                        }

                        if (Array.isArray(value)) {
                            value.forEach(item => {
                                params.append(key, item);
                            });
                        }
                        else {
                            params.append(key, value);
                        }
                    });
                }

                const response = await httpClient.request({url, method, params: params, data: body});

                setError(null);
                setErrorMessage("");
                setErrorState(null);

                if (typeof action === "function") {await action(response.data);}
            }
            catch (error) {
                let message;

                if (axios.isAxiosError(error)){
                    // Check if it's a network error (server down, CORS failure, offline)
                    if (error.code === 'ERR_NETWORK' || (!error.response && error.request)) {
                        setErrorState(500);
                        message = "A szerver nem érhető el.";
                    } else if (error.response) {
                        // The server responded with a status code outside the 2xx range
                        setErrorState(error.response.status);
                        message = error.response.data?.message ??
                                  error.response.data?.error ??
                                  error.response.data ??
                                  error.message;
                    } else {
                        // Something happened in setting up the request that triggered an Error
                        setErrorState(500);
                        message = error.message;
                    }
                }
                else {
                    setErrorState(500);
                    message = error?.message ?? "Ismeretlen hiba";
                }

                setError(error);
                setErrorMessage(message);
                return null;
            }
            finally {
                if (!freeze) {setLoading(false);}
            }
        }, [url, queries, method, body, action]
    );

    return {sendRequest};
}