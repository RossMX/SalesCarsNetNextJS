import { numberWithCommas } from '@/lib/helpers';
import { Auction, AuctionFinished } from '@/types';
import Image from 'next/image';
import Link from 'next/link';

type Props = {
    auction: Auction;
    finished: AuctionFinished;
}

export default function AuctionCreatedToast({auction, finished}: Props) {
  return (
    <Link href={`/auctions/details/${auction.id}`}
        className='flex flex-col items-center'>
        <div className='flex flex-row items-center gap-2'>
            <Image 
                src={auction.imageUrl}
                alt={'image of ' + auction.make}
                height={80}
                width={80}
                className='rounded-lg w-auto h-auto'
            />
            <div className='flex flex-col'>
                <span>Auction for {auction.make} {auction.model} has finished!</span>
                {finished.itemSold && finished.amount ? (
                    <p>Congrats to {finished.winner} who has won this auction 
                        for $${numberWithCommas(finished.amount)}</p>
                ) : (
                    <span>The auction did not meet the reserve price and the item was not sold.</span>
                )}
            </div>
        </div>
    </Link>
  )
}
