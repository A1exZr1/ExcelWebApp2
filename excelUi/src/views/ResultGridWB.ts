export default class ResultGridWB {
  constructor(
    public articleName: string,
    public sku: string,
    public supplierArticleName: string,
    public brand: string,
    public quantity: number,
    public retailPriceSumm: number,
    public amountPayableToSellerSumm: number,

    public logisticsFee: number,
    public cancelledQuantity: number,
    public cancelledSumm: number,
    public paidAcceptanceSumm: number,
    public totalAmountOfFines: number,
    public returnedQuantity: number,
    public returnedSumm: number,
    public advertisingCost: number,
    public reviewPointsCost: number,
    public cancellationWorkCost: number,
    public workCost: number | null,
    public materialCost: number | null,
    public netProfit: number,
    public profitPercent: number,
  ) {}
}
