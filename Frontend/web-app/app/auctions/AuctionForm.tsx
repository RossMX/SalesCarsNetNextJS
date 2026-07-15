'use client';

import { Button, Spinner } from "flowbite-react";
import { usePathname, useRouter } from "next/navigation";
import { FieldValues, useForm } from "react-hook-form"
import Input from "../components/Input";
import { useEffect } from "react";
import DateInput from "../components/DateInput";
import { createAuction, updateAuction } from "../actions/auctionsActions";
import toast from "react-hot-toast";
import { Auction } from "@/types";

type Props = {
  auction?: Auction
}

export default function AuctionForm({ auction }: Props) {
  const router = useRouter();
  const pathName = usePathname();

  const { control, handleSubmit, setFocus, reset,
    formState: { isSubmitting, isValid, isDirty } } = useForm({
      mode: 'onTouched'
    });

  useEffect(() => {
    if (auction) {
      const { make, model, color, year, mileage } = auction;
      reset({ make, model, color, year, mileage });
    }
    setFocus('make');
  }, [setFocus, auction, reset]);


  async function onSubmit(data: FieldValues) {
    try {
      let id = '';
      let res;

      if (pathName === '/auction/create') {
        res = await createAuction(data);
        id = res.id;
      } else {
        if (auction) {
          res = await updateAuction(data, auction.id);
          id = auction.id;
        }
      }

      if (res.error) {
        throw res.error;
      }
      router.push(`/auctions/details/${id}`);
    } catch (error: any) {
      //console.error('Error submitting form:', error);
      toast.error(error.status + ' ' + error.message);
    }
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col mt-3">

      {/*<div className="mb-3 block">
         <TextInput
          {...register('make', { required: 'Make is required'})}
          placeholder="Make"
          color={errors?.make && 'failure'}
        />

        <HelperText color="failure">
          {errors.make?.message as string}
        </HelperText>
      </div>

      <div className="mb-3 block">
        <TextInput
          {...register('model', { required: 'Model is required'})}
          placeholder="Model"
          color={errors?.model && 'failure'}
        />

        <HelperText color="failure">
          {errors.model?.message as string}
        </HelperText>
      </div> */}

      <Input name='make' label='Make' control={control} rules={{ required: 'Make is required' }} />
      <Input name='model' label='Model' control={control} rules={{ required: 'Model is required' }} />
      <Input name='color' label='Color' control={control} rules={{ required: 'Color is required' }} />

      <div className="grid grid-cols-2 gap-3">
        <Input name='year' label='Year' control={control} type='number' rules={{ required: 'Year is required' }} />
        <Input name='mileage' label='Mileage' control={control} type='number' rules={{ required: 'Mileage is required' }} />
      </div>

      {pathName === '/auction/create' &&
        <>
          <Input name='imageUrl' label='Image URL' control={control} rules={{ required: 'Image URL is required' }} />

          <div className="grid grid-cols-2 gap-3">
            <Input name='reservePrice' label='Reserve Price (enter 0 if no reserve)' control={control} type='number' rules={{ required: 'Reserve Price is required' }} />
            <DateInput
              name='auctionEnd'
              label='Auction end date/time'
              control={control}
              showTimeSelect
              dateFormat="dd MMMM yyyy, h:mm aa"
              rules={{ required: 'Auction end date is required' }} />
          </div>
        </>
      }

      <div className="flex justify-between">
        <Button color='alternative' onClick={() => router.push('/')}>Cancel</Button>
        <Button
          outline
          color='green'
          type='submit'
          disabled={!isValid || !isDirty}
        >
          {isSubmitting && <Spinner size="sm" />}
          Submit
        </Button>
      </div>
    </form>
  )
}
