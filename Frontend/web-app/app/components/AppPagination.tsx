'use client'
import { Pagination } from 'flowbite-react'

type Props = {
    currentPage: number;
    pageCount: number;
    onPageChanged: (page: number) => void;
}

export default function AppPagination({ currentPage, pageCount, onPageChanged }: Props) {
    return (
        <Pagination
            currentPage={currentPage}
            onPageChange={e => onPageChanged(e)}
            totalPages={pageCount}
            layout="pagination"
            showIcons={true}
            className="text-blue-500 mb-5"
        />
    )
}
