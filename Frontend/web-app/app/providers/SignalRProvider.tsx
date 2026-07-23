'use client';

import { useAuctionStore } from '@/hooks/useAuctionStore';
import { useBidStore } from '@/hooks/useBidStore';
import { Auction, AuctionFinished, Bid } from '@/types';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr'
import { User } from 'next-auth';
import { useParams } from 'next/navigation';
import { ReactNode, useCallback, useEffect, useRef } from 'react'
import toast from 'react-hot-toast';
import AuctionCreatedToast from '../components/AuctionCreatedToast';
import { getDetailedViewData } from '../actions/auctionsActions';
import AuctionFinishedToast from '../components/AuctionFinishedToast';

type Props = {
    children: ReactNode
    user: User | null
}

export default function SignalRProvider({ children, user }: Props) {
    const connection = useRef<HubConnection | null>(null);
    const setCurrentPrice = useAuctionStore(state => state.setCurrentPrice);
    const addBid = useBidStore(state => state.addBid);
    const params = useParams<{ id: string }>();

    const handleAuctionCreated = useCallback((auction: Auction) => {
        if (user?.username !== auction.seller) {
            return toast(<AuctionCreatedToast auction={auction} />, {
                duration: 10000,
            });
        }
    }, [user?.username]);

    const handleAuctionFinished = useCallback((auctionFinished: AuctionFinished) => {
        const auction = getDetailedViewData(auctionFinished.auctionId);
        toast.promise(auction, {
            loading: ' Loading',
            success: (auction) => 
                <AuctionFinishedToast 
                    auction={auction}
                    finished={auctionFinished}
                />,
            error: (err) => 'Auction finished'
        },{
            success: {
                duration: 10000,
                icon: null
            }
        });
    }, []);

    // Callback function to handle the BidPlaced event, and dont recreate it on every render
    const handleBidPlace = useCallback((bid: Bid) => {
        console.log('BidPlaced event received:', bid);

        if (bid.bidStatus.includes('Accepted')) {
            setCurrentPrice(bid.auctionId, bid.amount);
        }

        if (params.id == bid.auctionId) {
            addBid(bid);
        }
    }, [setCurrentPrice, addBid, params.id]);


    useEffect(() => {
        if (!connection.current) {
            connection.current = new HubConnectionBuilder()
                .withUrl('http://localhost:6001/notifications')
                .withAutomaticReconnect()
                .build();

            connection.current.start()
                .then(() => console.log('SignalR Connection established'))
                .catch((err) => console.log(err.message));
        }

        connection.current.on('BidPlaced', handleBidPlace);
        connection.current.on('AuctionCreated', handleAuctionCreated);
        connection.current.on('AuctionFinished', handleAuctionFinished);

        return () => {
            connection.current?.off('BidPlaced', handleBidPlace);
            connection.current?.off('AuctionCreated', handleAuctionCreated);
            connection.current?.off('AuctionFinished', handleAuctionFinished);
        }

    }, [setCurrentPrice, handleBidPlace, handleAuctionCreated, handleAuctionFinished]);

    return (
        children
    )
}
