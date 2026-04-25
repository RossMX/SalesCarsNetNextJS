'use server';

import { Auction, PagedResult } from "@/types";

// export async function getData(pageNumber: number, pageSize: number) : Promise<PagedResult<Auction>> {
export async function getData(query: string) : Promise<PagedResult<Auction>> {
    // const response = await fetch(`http://localhost:6001/search?pageSize=${pageSize}&pageNumber=${pageNumber}&filterBy=all`);
    const response = await fetch(`http://localhost:6001/search${query}`);

    if (!response.ok) {
        throw new Error('Failed to fetch auctions');
    }

    return response.json();
}
