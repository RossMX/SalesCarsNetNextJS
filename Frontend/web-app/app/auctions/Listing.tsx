'use client';

import AuctionCard from "./AuctionCard";
import AppPagination from "../components/AppPagination";
import { useEffect, useState } from "react";
import { getData } from "../actions/auctionsActions";
import Filters from "./Filters";
import { useParamsStore } from "@/hooks/useParamsStore";
import { useShallow } from "zustand/shallow";
import qs from "query-string";
import EmptyFilter from "../components/EmptyFilter";
import { useAuctionStore } from "@/hooks/useAuctionStore";

export default function Listing() {
  //const [auctions, setAuctions] = useState<Auction[]>([]);
  // const [pageCount, setPageCount] = useState(0);
  // const [pageNumber, setPageNumber] = useState(1);
  // const [pageSize, setPageSize] = useState(4);

  //const [data, setData] = useState<PagedResult<Auction>>();

  const [loading, setLoading] = useState(true);

  const data = useAuctionStore(useShallow(state => ({
    auctions: state.auctions,
    totalCount: state.totalCount,
    pageCount: state.pageCount
  })));

  const setData = useAuctionStore(state => state.setData);

  const params = useParamsStore(useShallow(state => ({
    pageNumber: state.pageNumber,
    pageSize: state.pageSize,
    searchTerm: state.searchTerm,
    orderBy: state.orderBy,
    filterBy: state.filterBy,
    seller: state.seller,
    winner: state.winner
  })));

  const setParams = useParamsStore(state => state.setParams);

  const url = qs.stringifyUrl({
    url: '',
    query: params
  }, { skipEmptyString: true });

  function setPageNumber(pageNumber: number) {
    setParams({ pageNumber });
  }

  useEffect(() => {
    getData(url).then(data => {
      //console.log(data);
      setData(data);
      setLoading(false);
    })
  }, [url, setData]);

  if (loading) {
    return <div>Loading...</div>;
  }

  return (
    <>
      <Filters />

      {data.totalCount === 0 ? (
        <EmptyFilter />
      ) : (
        <>
          <div className=" grid grid-cols-4 gap-6">
            {data && data.auctions.map(auction => (
              <AuctionCard key={auction.id} auction={auction} />
            ))}
          </div>

          <div className="flex justify-center mt-4">
            {data.auctions.length > 0 && (
              <AppPagination
                currentPage={params.pageNumber}
                pageCount={data.pageCount}
                onPageChanged={setPageNumber}
              />
            )}
          </div>
        </>
      )}
    </>
  )
}
