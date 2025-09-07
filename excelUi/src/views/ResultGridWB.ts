export default class ResultGridOzonV1 {
  constructor(
    public articleName: string,
    public sku: string,
    public quantity: number,
    public totalSumm: number,
    public revenue: number,
    public advertisingCost: number,
    public workCost: number | null,
    public materialCost: number | null,
    public unlinkedExpenses: number,
    public netProfit: number,
    public profitPercent: number,
  ) {}
}
