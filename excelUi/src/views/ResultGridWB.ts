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
    public returnMaterialDamageCost: number,
    public advertisingCost: number,
    public reviewPointsCost: number,
    public cancellationWorkQuantity: number,
    public cancellationWorkCost: number,
    public cancellationMaterialDamageCost: number,
    public workCost: number | null,
    public materialCost: number | null,
    public netProfit: number,
    public profitPercent: number | null,
  ) {}
}
