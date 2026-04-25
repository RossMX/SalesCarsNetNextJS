'use client'
import { Auction } from "@/types";
import Image from 'next/image'
import { useState } from 'react';

type Props = {
    auction: Auction;
}

export default function CardImage({ auction }: Props) {
 const [isLoading, setIsLoading] = useState(true);

  return (
    <Image 
        src={auction.imageUrl} 
        alt="Image of car" 
        fill 
        className={`object-cover duration-700 ease-in-out
        ${isLoading ? 'opacity-0 scale-110' : 'opacity-100 scale-100'}
        `}
        sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 25vw"
        onLoad={() => setIsLoading(false)} />
  )
}
