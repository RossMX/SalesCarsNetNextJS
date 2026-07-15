'use server';

import { fetchWrapper } from "@/lib/fetchWrapper";
import { Auction, PagedResult } from "@/types";
import { FieldValues } from "react-hook-form";

// export async function getData(pageNumber: number, pageSize: number) : Promise<PagedResult<Auction>> {
export async function getData(query: string) : Promise<PagedResult<Auction>> {
    // const response = await fetch(`http://localhost:6001/search?pageSize=${pageSize}&pageNumber=${pageNumber}&filterBy=all`);
    // const response = await fetch(`http://localhost:6001/search${query}`);

    // if (!response.ok) {
    //     throw new Error('Failed to fetch auctions');
    // }

    // return response.json();
    return fetchWrapper.get(`/search${query}`);
}

export async function updateAuctionTest(): Promise<{status: number, message: string}> {
    const data = {
        mileage: Math.floor(Math.random() * 100000) + 1
    };
    
    // const session = await auth();
    // const result = await fetch(`http://localhost:6001/auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c`, {
    //     method: 'PUT',
    //     headers: {
    //         'Content-Type': 'application/json',
    //         'Authorization': `Bearer ${session?.accessToken}`
    //     },
    //     body: JSON.stringify(data)
    // });
    // if (!result.ok) {
    //     return {status: result.status, message: result.statusText};
    // }
    // return {status: result.status, message: result.statusText};

    return fetchWrapper.put(`/auctions/afbee524-5972-4075-8800-7d1f9d7b0a0c`, data);
}

export async function createAuction(data: FieldValues) {
    return fetchWrapper.post('/auctions', data);
}

export async function getDetailedViewData(id: string) : Promise<Auction> {
    return fetchWrapper.get(`/auctions/${id}`);
}

export async function updateAuction(data: FieldValues, id: string) {
    return fetchWrapper.put(`/auctions/${id}`, data);
}

export async function deleteAuction(id: string) {
    return fetchWrapper.del(`/auctions/${id}`);
}