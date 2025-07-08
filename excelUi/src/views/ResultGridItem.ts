export default class ResultGridItem {
  constructor(
    public articleName: string,
    public sku: string,
    public revenue: number,
    public advertisingCost: number,
    public primeCost: number,
    public netProfit: number,
    public profitPercent: number
  ) {}
}