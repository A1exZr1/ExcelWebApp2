export default class ResultGridOzonV1 {
  constructor(
    public articleName: string,
    public sku: string,
    public warehouse: string,
    public preCommissionAmount: number,
    public quantity: number,

    public workCost: number | null,
    public materialCost: number | null,
    public unlinkedExpenses: number,

    public ozonFee: number,
    public handlingFee: number,
    public lastMileFee: number,
    public logisticsFee: number,
    public netProfit: number,
    public profitPercent: number | null,

    public additionalFees: Record<string, number> = {},
  ) {}
}
