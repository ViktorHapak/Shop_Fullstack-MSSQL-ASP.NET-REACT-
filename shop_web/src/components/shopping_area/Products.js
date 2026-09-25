import {Link, useNavigate} from "react-router";
import {Fragment, useCallback, useContext, useEffect, useMemo, useState} from "react";
import {useStateContext} from "../../_context/context_provider";
import styles from './Products.module.css';
import ProductCard from "./ProductCard";
import productImageUrl from "../../assets/product.webp";
import {useHttp} from "../../_client/axios";
import ProductForm from "./productform/ProductForm";
import CleanDepartmentForm from "./CleanDepartmentForm";

function Products() {

    const {products, categories = [], parameters,
        minPrice, maxPrice, pages,
        error, errorMessage, errorState, loading,
        token, role, authorities,
        setProducts, setParameters, setMinPrice, setMaxPrice, setCategories, setPages,
        setError, setErrorMessage, setErrorState, setLoading, setNotification} = useStateContext();

    const [showCategories, setShowCategories] = useState(false);
    const [displayedСategories, setDisplayedСategories] = useState([]);
    const [showPriceFilter, setShowPriceFilter] = useState(false);
    const [min, setMin] = useState(0);
    const [max, setMax] = useState(0);

    const [actualPage, setActualPage] = useState(0);
    const [pageNumbers, setPageNumbers] = useState([]);
    const [editId, setEditId] = useState(0);

    const [productForm, setProductForm] = useState(false);
    const [newCategoryName, setNewCategoryName] = useState("");
    const [cleanDepartmentId, setCleanDepartmentId] = useState(0);
    const [addDepartment, setAddDepartment] = useState(false);

    const navigate = useNavigate();

    let emptyProductBlob = null;

    useEffect(() => {
        console.log("Categories at products", categories);
        setDisplayedСategories(categories?.map(category => category.name) ?? []);
    }, [categories])

    useEffect(() => {
        setMin(Number(minPrice ?? 0));
        setMax(Number(maxPrice ?? 0));
    }, [minPrice, maxPrice]);

    useEffect(() => {
        if (!pages || pages <= 0) {
            setPageNumbers([]);
            return;
        }

        const visiblePages = 5;

        // No more 5 pages
        if (pages <= visiblePages) {
            setPageNumbers(
                Array.from({ length: pages }, (_, index) => index)
            );
            return;
        }

        let startPage;

        // At start of page-list
        if (actualPage <= 2) {
            startPage = 0;
        }

        //At ending of page-lis
        else if (actualPage >= pages - 3) {
            startPage = pages - visiblePages;
        }

        //At middle, actualPage = 5 -> [3, 4, 5, 6, 7]
        else {
            startPage = actualPage - 2;
        }

        setPageNumbers(
            Array.from(
                { length: visiblePages },
                (_, index) => startPage + index
            )
        );
    }, [pages, actualPage]);

    const getProducts = useCallback(async data => {
        const defaultBlob = await loadDefaultProductBlob();

        const _products = data?.items?.map(item => {
            return {
                id: item.id,
                name: item.name,
                price: item.price,
                stock: item.stock,
                departmentId: item.departmentId,
                department: item.department?.name ?? null,
                dataUrl: URL.createObjectURL(defaultBlob),
            }
        }) ?? [];


        setProducts(_products);
        setPages(data?.totalPages ?? 0);

        setMinPrice(Number(data?.min ?? 0));
        setMaxPrice(Number(data?.max ?? 0));

        console.log(_products)
    }, []);

    const getCategories = useCallback(async data => {
        const _categories = data ?? [];
        setCategories(_categories);
        setShowCategories(false);
        setCleanDepartmentId(0);
        setNewCategoryName("");
    }, []);

    const createCategory = useCallback(async data => {
        setNotification(`Új részleg létrehozva!\n ${newCategoryName}`);
        sendCategoriesRequest();
    })

    const deleteProducts = useCallback(async data => {
        setNotification("Minden termék törölve!");
        sendProductsRequest();
    })

    const deleteDepartments = useCallback(async data => {
        setNotification("Minden részleg törölve!");
        sendProductsRequest();
        sendCategoriesRequest()
    })

    const {
        sendRequest: sendProductsRequest
    } = useHttp('products', parameters, 'GET', null, getProducts);

    const {
        sendRequest: sendCategoriesRequest
    } = useHttp('products/dep/all', null, 'GET', null, getCategories);

    const {
        sendRequest: createCategoryRequest
    } = useHttp('products/dep', null, 'POST', {name: newCategoryName}, createCategory);

    const {
        sendRequest: removeAllProductsRequest,
    } = useHttp('products/dep/all', null, 'DELETE', null, deleteProducts);

    const {
        sendRequest: removeAllDepartmentsRequest,
    } = useHttp('products/dep/all', null, 'DELETE', null, deleteDepartments);

    useEffect(() => {
        sendProductsRequest();
    }, [parameters]);

    useEffect(() => {
        sendCategoriesRequest();
    }, [sendCategoriesRequest]);

    useEffect(() => {
        setParameters(prev => ({ ...prev, department_name: null }));
        setLoading(true);
    }, []);

    useEffect(() => {
        if (errorState === 401 && error) {
            setNotification('A művelethez jelentkezzen be!');
            navigate('/main/profile/login');
        }
    }, [errorState]);

    useEffect(() => {
        setTimeout(() => {
            if (errorState !== 500 ) {
                setError(null);
                setErrorMessage('');
                setErrorState(null);
            }

            setCleanDepartmentId(0);
        }, 5000)
    }, [error])

    useEffect(() => {
        return () => {
            setError(null);
            setLoading(false);
            setErrorMessage('');
        };
    }, [setError, setLoading, setErrorMessage]);

    const loadDefaultProductBlob = async () => {
        const response = await fetch(productImageUrl);

        if (!response.ok) {
            throw new Error("Product is not loaded yet");
        }

        return await response.blob();
    };

    const deleteDepartment = async (categorId, categoryName) => {
        try{
            const _noticification = await fetch(
                `http://localhost:5167/api/products/dep/${categorId}`,
                {
                    method: "DELETE",
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                })
                .then(res => {
                    if (!res.ok) {
                        throw new Error(`HTTP error: ${res.status}`);
                    }

                    //setItems(null);

                    return `Részleg törölve: \n${categoryName}`;
                })
                .catch(err => {
                    setError(err)
                    setErrorState(error.response.status);
                    let _errorMessage = error.response.data?.message ?? error.response.data?.error ??
                        error.response.data ?? error.message;

                    setErrorMessage(`${_errorMessage}`);
                    setLoading(false);
                });

            if (_noticification && !error) setNotification(_noticification);
            await sendCategoriesRequest();
            await sendProductsRequest();

        } catch (error) {
            console.error(error);
        } finally {
            setLoading(false);

        }
    }

    /*const orderProducts = (option) => {
        let _products = products ?? [];
        switch (option) {
            case "default": break;
            case "name_inc": {
                _products = [..._products].sort((a, b) => a.name.localeCompare(b.name));
                break;
            }
            case "name_dec": {
                _products = [..._products].sort((a, b) => b.name.localeCompare(a.name));
                break;
            }
            case "price_inc": {
                _products = [..._products].sort((a, b) => a.price - b.price);
                break;
            }
            case "price_dec": {
                _products = [..._products].sort((a, b) => b.price - a.price);
                break;
            }
            default: break;
        }

        setOrder(order);
        setProducts(_products);
    };*/

    const addProductEnabled = () => {
        return (
            role === "Admin" ||
            (role === "Moderator" && authorities?.addProduct === true)
        );
    };
    const updateProductEnabled = () => {
        return (
            role === "Admin" ||
            (role === "Moderator" && authorities?.updateProduct === true)
        );
    };
    const deleteProductEnabled = () => {
        return (
            role === "Admin" ||
            (role === "Moderator" && authorities?.deleteProduct === true)
        );
    };
    const addDepartmentEnabled = () => {
        return (
            role === "Admin" ||
            (role === "Moderator" && authorities?.addDepartment === true)
        );
    };
    const deleteDepartmentEnabled = () => {
        return (
            role === "Admin" ||
            (role === "Moderator" && authorities?.deleteDepartment === true)
        );
    };
    const replenishStackEnabled = () => {
        return (
            role === "Admin" ||
            (role === "Moderator" && authorities?.replenishStack === true)
        );
    };
    const reduceStackEnabled = () => {
        return (
            role === "Admin" ||
            (role === "Moderator" && authorities?.reduceStack === true)
        );
    };

    return (
        <div className={styles["products-container"]}>
            <div className={`${styles["navbar"]} ${styles["filter-nav"]}`}>
                <ul className={`${styles["nav-items"]} ${styles["left"]}`}>
                    <li className={styles["nav-item"]}
                        onMouseEnter={() => setShowCategories(true)}
                        onMouseLeave={() => setShowCategories(false)}
                    >
                        <a className={`${styles["categories-filter"]} ${showCategories ? styles["active"] : ""}`}>
                            <i className="fa fa-bars" />Kategóriák
                        </a>

                        {errorState !== 500 && showCategories && (
                            <div className={styles["categories-list-container"]}>
                                {categories.map(category => (
                                        <div
                                            key={category.id}
                                            className={styles["category-item"]}
                                        >
                                            <a className={styles["category-title"]}
                                               onClick={() => {
                                                setParameters({
                                                    ...parameters,
                                                    page: 0,
                                                    department_name: category?.name ?? ""
                                                });
                                                setShowCategories(false);
                                            }}
                                            >
                                                {category?.name ?? ""}
                                            </a>
                                            {deleteProductEnabled() && (
                                                <div className={styles["category-clean-div"]}>
                                                    <button className={`${styles["category-form-button"]} btn btn-warning`}
                                                            onClick={() => {setCleanDepartmentId(category.id)}}
                                                    >
                                                        <i className="fa fa-trash"/>
                                                    </button>
                                                    <button className={`${styles["category-form-button"]} btn btn-danger`}
                                                            onClick={() => {deleteDepartment(category.id, category.name)}}
                                                            disabled={!deleteDepartmentEnabled()}
                                                    >
                                                        <i className="fa fa-minus"/>
                                                    </button>
                                                </div>
                                            )}
                                        </div>
                                    )
                                )}

                                {addDepartmentEnabled && (
                                    <div className={styles["category-form-container"]}>
                                        {!addDepartment ? (
                                            <button className={`${styles["category-form-open"]} btn btn-primary`}
                                                    onClick={() => setAddDepartment(true)}>
                                                Új kategória
                                            </button>
                                        ) : (
                                            <div className={styles["category-form"]}>
                                                <button type="button" className={styles["close-category-form"]}
                                                        onClick={() => setAddDepartment(false)}
                                                >
                                                    <i className="fa fa-close" />
                                                </button>

                                                <input type="text" className={styles["category-form-input"]} placeholder="Elnevezés"
                                                       onChange={(e) => setNewCategoryName(e.target.value)}/>

                                                <button type="button" className={`${styles["category-form-submit"]} btn btn-success`}
                                                        onClick={() => createCategoryRequest()}
                                                        disabled={!addDepartmentEnabled()}>
                                                    <i className="fa fa-plus" />
                                                </button>
                                            </div>
                                        )}

                                    </div>
                                )}
                            </div>
                         )
                        }

                    </li>

                    <li className={styles["nav-item"]}
                        onMouseEnter={() => setShowPriceFilter(true)}
                        onMouseLeave={() => setShowPriceFilter(false)}>
                        <a className={styles["price-filter"]}>
                            Ár
                        </a>

                        {errorState !== 500 && showPriceFilter && (
                            <div className={styles["price-filter-container"]}>
                                <div className={styles["min-price-div"]}>
                                    <label className={styles["price-label"]}>Min</label>
                                    <input className={styles["price-input"]} type={"number"} value={min}
                                           onChange={event => {
                                               setMin(event.target.value);
                                               setParameters({
                                                   ...parameters,
                                                   page: 0,
                                                   minPrice: min
                                               });
                                           }} />
                                </div>
                                <div className={styles["max-price-div"]}>
                                    <label className={styles["price-label"]}>Max</label>
                                    <input className={styles["price-input"]} type={"number"} value={max}
                                           onChange={event => {
                                               setMax(event.target.value);
                                               setParameters({
                                                   ...parameters,
                                                   page: 0,
                                                   maxPrice: max,
                                               });
                                           }} />
                                </div>
                            </div>
                        )}
                    </li>
                </ul>
                {token && (
                    <ul className={`${styles["nav-items"]} ${styles["right"]}`}>
                        <li className={styles["nav-item"]}>
                            <button className="btn btn-success fw-bold"
                                    onClick={() => setProductForm(true)}
                                    disabled={!addProductEnabled()}
                            >
                                Új termék
                            </button>
                        </li>
                    </ul>
                )}
            </div>

            {errorState === 500 && (
                <div className="alert alert-warning m-4">Szerver hiba!</div>
            )}

            {errorState !== 500 && (
                <div className={styles["main-page"]}>
                    <div className={`${styles["options-div"]}`}>
                        <div className={`${styles["order-div"]}`}>
                            <label className={`${styles["option-label"]}`} htmlFor="order-options">Rendezés:</label>
                            <select className={`${styles["order-select"]}`}
                                    value={parameters.order}
                                    onChange={(e) =>
                                        setParameters(() => ({
                                            ...({order: e.target.value})
                                        }))
                                    }
                                    id="order-options"
                            >
                                <option value="default"></option>
                                <option value="name_inc">Elnevezés szerint növekvő</option>
                                <option value="name_dec">Elnevezés szerint csökkenő</option>
                                <option value="price_inc">Ár szerint növekvő</option>
                                <option value="price_dec">Ár szerint csökkenő</option>
                            </select>
                        </div>
                        <div className={`${styles["pagesize-div"]}`}>
                            <label className={`${styles["option-label"]}`} htmlFor="pagesize-option">Termék/oldal:</label>
                            <input type="number" className={`${styles["pagesize-input"]}`} id="pagesize-option"
                                   value={parameters.size}
                                   onChange={event => {
                                       setParameters({
                                           ...parameters,
                                           page: 0,
                                           size: event.target.value
                                       });
                                   }}
                            />
                        </div>

                    </div>
                    <hr/>
                    <div className={`${styles["pages-row"]}`}>
                        <div className={`${styles["pages-select"]}`}>
                            {pageNumbers.map(pageNumber =>
                                (
                                    <button className={`${styles["page-point"]} 
                                                    ${(pageNumber == actualPage) ? styles["active"] : ""}`}
                                            onClick={() => {
                                                setParameters({
                                                    ...parameters,
                                                    page: pageNumber,
                                                });
                                                setActualPage(pageNumber);
                                            }}
                                            disabled={pageNumber == actualPage}/>
                                )
                            )}
                        </div>
                    </div>

                    {loading &&
                        (<div className={`${styles["loading-container"]}`}>
                            <span className={`${styles["loading-title"]}`}>
                                <label>Loading</label>
                                <label className={`${styles["dot-first"]}`}>.</label>
                                <label className={`${styles["dot-second"]}`}>.</label>
                                <label className={`${styles["dot-third"]}`}>.</label>
                            </span>
                        </div>)
                    }
                    {!loading && error && !productForm && (
                        <div className="alert alert-danger m-4">{errorMessage}</div>
                    )}

                    {!loading  && products?.length > 0 &&
                        (
                            <div className={styles["products-grid"]}>
                                {products.map(product => (
                                    <ProductCard
                                        key={product.id}
                                        product={product}
                                        getProducts={sendProductsRequest}
                                        editId={editId}
                                        setEditId={setEditId}
                                        addProductEnabled={addProductEnabled()}
                                        updateProductEnabled={updateProductEnabled()}
                                        deleteProductEnabled={deleteProductEnabled()}
                                        addDepartmentEnabled={addDepartmentEnabled()}
                                        deleteDepartmentEnabled={deleteDepartmentEnabled()}
                                        replenishStackEnabled={replenishStackEnabled()}
                                        reduceStackEnabled={reduceStackEnabled()}
                                    />
                                ))}
                            </div>
                        )
                    }

                    {!loading && !error && products?.length === 0 && (
                        <div className={styles["empty-message"]}>
                            Nincsenek megjeleníthető termékek!
                        </div>
                    )}

                    {token && role === "Admin" && (
                        <div className={`${styles["removeall-div"]}`}>
                            <button className="btn btn-dark" onClick={() => removeAllProductsRequest}>Minden termék törlése</button>
                            <button className="btn btn-dark" onClick={() => removeAllDepartmentsRequest}>Minden részleg törlése</button>
                        </div>
                    )}


                </div>
            )}

            {productForm === true && (
                <ProductForm
                    getProducts={sendProductsRequest}
                    closeForm={() => setProductForm(false)}
                />
            )}

            {productForm === true && (
                <div className={`${styles["black-page"]}`}></div>
            )}

            {cleanDepartmentId !== 0 && (
                <CleanDepartmentForm id={cleanDepartmentId}
                                     closeForm={() => setCleanDepartmentId(0)}
                                     getProducts={sendProductsRequest}>
                </CleanDepartmentForm>
            )}
        </div>
    );
}

export default Products;