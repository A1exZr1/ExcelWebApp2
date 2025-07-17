export default class ResultGridItem {
  constructor(
    public articleName: string,
    public sku: string,
    public quantity: number,
    public totalSummary: number,
    public revenue: number,
    public advertisingCost: number,
    public primeCost: number | null,
    public unlinkedExpenses: number,
    public netProfit: number,
    public profitPercent: number,
  ) {}
}
