import {Component} from "react";

export class Product{
    id;
    name;
    price;
    stock;
    departmentId;
    deparment;
    dataUrl;


    constructor(id, name, price, stock, departmentId,deparment) {
        this.id = id;
        this.name = name;
        this.price = price;
        this.stock = stock;
        this.departmentId = departmentId;
        this.deparment = deparment;
    }
}

